    /// Monad internal implementation of the ICommandRuntime2 interface
    /// used for execution in the monad engine environment.
    /// There will be one instance of this class for each cmdlet added to
    /// a pipeline. When the cmdlet calls its WriteObject API, that API will call
    /// the WriteObject implementation in this class which, in turn, calls
    /// the downstream cmdlet.
    internal class MshCommandRuntime : ICommandRuntime2
        /// Gets/Set the execution context value for this runtime object.
        internal ExecutionContext Context { get; set; }
        private SessionState _state = null;
        internal InternalHost CBhost;
        /// The host object for this object.
        public PSHost Host { get; }
        // Output pipes.
        private Pipe _inputPipe;
        private Pipe _outputPipe;
        private Pipe _errorOutputPipe;
        /// IsClosed indicates to the Cmdlet whether its upstream partner
        /// could still write more data to its incoming queue.
        /// Note that there may still be data in the incoming queue.
        internal bool IsClosed { get; set; }
        /// True if we're not closed and the input pipe is non-null...
        internal bool IsPipelineInputExpected
                // No objects in the input pipe
                // The pipe is closed. So there can't be any more object
                if (IsClosed && (_inputPipe == null || _inputPipe.Empty))
        /// This allows all success output to be set to a variable.  Similar to the way -errorvariable sets
        /// all errors to a variable name.  Semantically this is equivalent to :  cmd |set-var varname -passthru
        /// but it should be MUCH faster as there is no binding that takes place.
        /// This is a common parameter via class CommonParameters.
        internal string OutVariable { get; set; }
        internal IList OutVarList { get { return _outVarList; } set { _outVarList = value; } }
        private IList _outVarList = null;
        internal PipelineProcessor PipelineProcessor { get; set; }
        private readonly CommandInfo _commandInfo;
        private readonly InternalCommand _thisCommand;
        internal MshCommandRuntime(ExecutionContext context, CommandInfo commandInfo, InternalCommand thisCommand)
            this.CBhost = (InternalHost)context.EngineHostInterface;
            _commandInfo = commandInfo;
            _thisCommand = thisCommand;
            LogPipelineExecutionDetail = InitShouldLogPipelineExecutionDetail();
            if (_commandInfo != null)
                return _commandInfo.ToString();
        private InvocationInfo _myInvocation;
            get { return _myInvocation ??= _thisCommand.MyInvocation; }
            get { return (this.PipelineProcessor != null && this.PipelineProcessor.Stopping); }
        // Trust: WriteObject needs to respect EmitTrustCategory
        /// Writes the object to the output pipe.
        /// WriteObject may only be called during a call to this Cmdlet's
        /// <seealso cref="System.Management.Automation.ICommandRuntime.WriteObject(object,bool)"/>
        /// <seealso cref="System.Management.Automation.ICommandRuntime.WriteError(ErrorRecord)"/>
            // This check will be repeated in _WriteObjectSkipAllowCheck,
            // but we want PipelineStoppedException to take precedence
            // over InvalidOperationException if the pipeline has been
            // closed.
            ThrowIfStopping();
            // SecurityContext is not supported in CoreCLR
            DoWriteObject(sendToPipeline);
            if (UseSecurityContextRun)
                if (PipelineProcessor == null || PipelineProcessor.SecurityContext == null)
                    throw PSTraceSource.NewInvalidOperationException(PipelineStrings.WriteNotPermitted);
                ContextCallback delegateCallback =
                    new ContextCallback(DoWriteObject);
                SecurityContext.Run(
                    PipelineProcessor.SecurityContext.CreateCopy(),
                    delegateCallback,
                    sendToPipeline);
        /// Not permitted at this time or from this thread
        private void DoWriteObject(object sendToPipeline)
            ThrowIfWriteNotPermitted(true);
            _WriteObjectSkipAllowCheck(sendToPipeline);
        /// Writes one or more objects to the output pipe.
        /// <seealso cref="System.Management.Automation.ICommandRuntime.WriteObject(object)"/>
            if (!enumerateCollection)
                WriteObject(sendToPipeline);
            // This check will be repeated in _WriteObjectsSkipAllowCheck,
            DoWriteEnumeratedObject(sendToPipeline);
                    new ContextCallback(DoWriteObjects);
                DoWriteObjects(sendToPipeline);
        /// Writes an object enumerated from a collection to the output pipe.
        /// The enumerated object that needs to be written to the pipeline.
        private void DoWriteEnumeratedObject(object sendToPipeline)
            _EnumerateAndWriteObjectSkipAllowCheck(sendToPipeline);
        // Trust:  public void WriteObject(object sendToPipeline, DataTrustCategory trustCategory);     // enumerateCollection defaults to false
        // Trust:  public void WriteObject(object sendToPipeline, bool enumerateCollection, DataTrustCategory trustCategory);
        // Variables needed to generate a unique SourceId for
        // WriteProgress(ProgressRecord).
        private static Int64 s_lastUsedSourceId /* = 0 */;
        private Int64 _sourceId /* = 0 */;
        /// Display progress information.
        /// WriteProgress may only be called during a call to this Cmdlet's
        /// the activity of your Cmdlet, when the operation of your Cmdlet
        /// <seealso cref="System.Management.Automation.Cmdlet.WriteDebug(string)"/>
        /// <seealso cref="System.Management.Automation.Cmdlet.WriteWarning(string)"/>
        /// <seealso cref="System.Management.Automation.Cmdlet.WriteVerbose(string)"/>
        public void WriteProgress(ProgressRecord progressRecord)
            this.WriteProgress(progressRecord, false);
        internal void WriteProgress(ProgressRecord progressRecord, bool overrideInquire)
            // NTRAID#Windows Out Of Band Releases-918023-2005/08/22-JonN
            // WriteError/WriteObject have a check that prevents them to be called from outside
            // Begin/Process/End. This is done because the Pipeline needs to be ready before these
            // functions can be called.
            // WriteDebug/Warning/Verbose/Process used to do the same check, even though it is not
            // strictly needed. If we ever implement pipelines for these objects we may need to
            // enforce the check again.
            // See bug 583774 in the Windows 7 database for more details.
            ThrowIfWriteNotPermitted(false);
            // Bug909439: We need a unique sourceId to send to
            // WriteProgress. The following logic ensures that
            // there is a unique id for each Cmdlet instance.
            if (_sourceId == 0)
                _sourceId = Interlocked.Increment(ref s_lastUsedSourceId);
            this.WriteProgress(_sourceId, progressRecord, overrideInquire);
        /// If the pipeline is terminated due to ActionPreference.Stop
        /// or ActionPreference.Inquire, this method will throw
        /// <see cref="System.Management.Automation.PipelineStoppedException"/>,
        /// but the command failure will ultimately be
        /// <see cref="System.Management.Automation.ActionPreferenceStopException"/>,
        public void WriteProgress(
            Int64 sourceId,
            ProgressRecord progressRecord)
            WriteProgress(sourceId, progressRecord, false);
        internal bool IsWriteProgressEnabled()
            => WriteHelper_ShouldWrite(ProgressPreference, lastProgressContinueStatus);
        internal void WriteProgress(
                ProgressRecord progressRecord,
                bool overrideInquire)
            if (progressRecord == null)
                throw PSTraceSource.NewArgumentNullException(nameof(progressRecord));
            if (Host == null || Host.UI == null)
                Diagnostics.Assert(false, "No host in CommandBase.WriteProgress()");
            InternalHostUserInterface ui = Host.UI as InternalHostUserInterface;
            ActionPreference preference = ProgressPreference;
            if (overrideInquire && preference == ActionPreference.Inquire)
                preference = ActionPreference.Continue;
            if (WriteHelper_ShouldWrite(
                preference, lastProgressContinueStatus))
                // Break into the debugger if requested
                if (preference == ActionPreference.Break)
                    CBhost?.Runspace?.Debugger?.Break(progressRecord);
                ui.WriteProgress(sourceId, progressRecord);
            lastProgressContinueStatus = WriteHelper(
                preference,
                lastProgressContinueStatus,
                "ProgressPreference",
                progressRecord.Activity);
        /// WriteDebug may only be called during a call to this Cmdlet's
        /// Use WriteDebug to display debug information on the inner workings
        /// of your Cmdlet.  By default, debug output will
        /// DebugPreference shell variable or the -Debug command-line option.
        /// <seealso cref="System.Management.Automation.Cmdlet.WriteProgress(ProgressRecord)"/>
        public void WriteDebug(string text)
            WriteDebug(new DebugRecord(text));
        internal bool IsWriteDebugEnabled()
            => WriteHelper_ShouldWrite(DebugPreference, lastDebugContinueStatus);
        internal void WriteDebug(DebugRecord record, bool overrideInquire = false)
            ActionPreference preference = DebugPreference;
            if (WriteHelper_ShouldWrite(preference, lastDebugContinueStatus))
                if (record.InvocationInfo == null)
                    record.SetInvocationInfo(MyInvocation);
                    CBhost?.Runspace?.Debugger?.Break(record);
                if (DebugOutputPipe != null)
                    if (CBhost != null && CBhost.InternalUI != null &&
                        DebugOutputPipe.NullPipe)
                        // If redirecting to a null pipe, still write to
                        // information buffers.
                        CBhost.InternalUI.WriteDebugInfoBuffers(record);
                    // Set WriteStream so that the debug output is formatted correctly.
                    PSObject debugWrap = PSObject.AsPSObject(record);
                    debugWrap.WriteStream = WriteStreamType.Debug;
                    DebugOutputPipe.Add(debugWrap);
                    // If no pipe, write directly to host.
                        Diagnostics.Assert(false, "No host in CommandBase.WriteDebug()");
                    CBhost.InternalUI.TranscribeResult(StringUtil.Format(InternalHostUserInterfaceStrings.DebugFormatString, record.Message));
                    CBhost.InternalUI.WriteDebugRecord(record);
            lastDebugContinueStatus = WriteHelper(
                lastDebugContinueStatus,
                "DebugPreference",
                record.Message);
        /// Display verbose information.
        /// WriteVerbose may only be called during a call to this Cmdlet's
        /// Use WriteVerbose to display more detailed information about
        /// the activity of your Cmdlet.  By default, verbose output will
        public void WriteVerbose(string text)
            WriteVerbose(new VerboseRecord(text));
        internal bool IsWriteVerboseEnabled()
            => WriteHelper_ShouldWrite(VerbosePreference, lastVerboseContinueStatus);
        internal void WriteVerbose(VerboseRecord record, bool overrideInquire = false)
            ActionPreference preference = VerbosePreference;
            if (WriteHelper_ShouldWrite(preference, lastVerboseContinueStatus))
                if (VerboseOutputPipe != null)
                        VerboseOutputPipe.NullPipe)
                        CBhost.InternalUI.WriteVerboseInfoBuffers(record);
                    // Add WriteStream so that the verbose output is formatted correctly.
                    PSObject verboseWrap = PSObject.AsPSObject(record);
                    verboseWrap.WriteStream = WriteStreamType.Verbose;
                    VerboseOutputPipe.Add(verboseWrap);
                        Diagnostics.Assert(false, "No host in CommandBase.WriteVerbose()");
                    CBhost.InternalUI.TranscribeResult(StringUtil.Format(InternalHostUserInterfaceStrings.VerboseFormatString, record.Message));
                    CBhost.InternalUI.WriteVerboseRecord(record);
            lastVerboseContinueStatus = WriteHelper(
                lastVerboseContinueStatus,
                "VerbosePreference",
        /// Display warning information.
        /// WriteWarning may only be called during a call to this Cmdlet's
        public void WriteWarning(string text)
            WriteWarning(new WarningRecord(text));
        internal bool IsWriteWarningEnabled()
            => WriteHelper_ShouldWrite(WarningPreference, lastWarningContinueStatus);
        internal void WriteWarning(WarningRecord record, bool overrideInquire = false)
            ActionPreference preference = WarningPreference;
            if (WriteHelper_ShouldWrite(preference, lastWarningContinueStatus))
                if (WarningOutputPipe != null)
                        WarningOutputPipe.NullPipe)
                        CBhost.InternalUI.WriteWarningInfoBuffers(record);
                    // Add WriteStream so that the warning output is formatted correctly.
                    PSObject warningWrap = PSObject.AsPSObject(record);
                    warningWrap.WriteStream = WriteStreamType.Warning;
                    WarningOutputPipe.AddWithoutAppendingOutVarList(warningWrap);
                        Diagnostics.Assert(false, "No host in CommandBase.WriteWarning()");
                    CBhost.InternalUI.TranscribeResult(StringUtil.Format(InternalHostUserInterfaceStrings.WarningFormatString, record.Message));
                    CBhost.InternalUI.WriteWarningRecord(record);
            AppendWarningVarList(record);
            lastWarningContinueStatus = WriteHelper(
                lastWarningContinueStatus,
                "WarningPreference",
        /// Display tagged object information.
        public void WriteInformation(InformationRecord informationRecord)
            WriteInformation(informationRecord, false);
        internal bool IsWriteInformationEnabled()
            => WriteHelper_ShouldWrite(InformationPreference, lastInformationContinueStatus);
        internal void WriteInformation(InformationRecord record, bool overrideInquire = false)
            ActionPreference preference = InformationPreference;
            if (preference != ActionPreference.Ignore)
                if (InformationOutputPipe != null)
                        InformationOutputPipe.NullPipe)
                        CBhost.InternalUI.WriteInformationInfoBuffers(record);
                    // Add WriteStream so that the information output is formatted correctly.
                    PSObject informationWrap = PSObject.AsPSObject(record);
                    informationWrap.WriteStream = WriteStreamType.Information;
                    InformationOutputPipe.Add(informationWrap);
                        throw PSTraceSource.NewInvalidOperationException("No host in CommandBase.WriteInformation()");
                    CBhost.InternalUI.WriteInformationRecord(record);
                    if ((record.Tags.Contains("PSHOST") && (!record.Tags.Contains("FORWARDED")))
                        || (preference == ActionPreference.Continue))
                        HostInformationMessage hostOutput = record.MessageData as HostInformationMessage;
                        if (hostOutput != null)
                            string message = hostOutput.Message;
                            ConsoleColor? foregroundColor = null;
                            ConsoleColor? backgroundColor = null;
                            bool noNewLine = false;
                            if (hostOutput.ForegroundColor.HasValue)
                                foregroundColor = hostOutput.ForegroundColor.Value;
                            if (hostOutput.BackgroundColor.HasValue)
                                backgroundColor = hostOutput.BackgroundColor.Value;
                            if (hostOutput.NoNewLine.HasValue)
                                noNewLine = hostOutput.NoNewLine.Value;
                            if (foregroundColor.HasValue || backgroundColor.HasValue)
                                // It is possible for either one or the other to be empty if run from a
                                // non-interactive host, but only one was specified in Write-Host.
                                // So fill them with defaults if they are empty.
                                if (!foregroundColor.HasValue)
                                    foregroundColor = ConsoleColor.Gray;
                                if (!backgroundColor.HasValue)
                                    backgroundColor = ConsoleColor.Black;
                                if (noNewLine)
                                    CBhost.InternalUI.Write(foregroundColor.Value, backgroundColor.Value, message);
                                    CBhost.InternalUI.WriteLine(foregroundColor.Value, backgroundColor.Value, message);
                                    CBhost.InternalUI.Write(message);
                                    CBhost.InternalUI.WriteLine(message);
                            CBhost.InternalUI.WriteLine(record.ToString());
                // Both informational and PSHost-targeted messages are transcribed here.
                // The only difference between these two is that PSHost-targeted messages are transcribed
                // even if InformationAction is SilentlyContinue.
                if (record.Tags.Contains("PSHOST") || (preference != ActionPreference.SilentlyContinue))
                    CBhost.InternalUI.TranscribeResult(record.ToString());
            AppendInformationVarList(record);
            lastInformationContinueStatus = WriteHelper(
                lastInformationContinueStatus,
                "InformationPreference",
                record.ToString());
        public void WriteCommandDetail(string text)
            this.PipelineProcessor.LogExecutionInfo(_thisCommand.MyInvocation, text);
        internal bool LogPipelineExecutionDetail { get; } = false;
        private bool InitShouldLogPipelineExecutionDetail()
            CmdletInfo cmdletInfo = _commandInfo as CmdletInfo;
                if (string.Equals("Add-Type", cmdletInfo.Name, StringComparison.OrdinalIgnoreCase))
                if (cmdletInfo.Module == null && cmdletInfo.PSSnapIn != null)
                    return cmdletInfo.PSSnapIn.LogPipelineExecutionDetails;
                if (cmdletInfo.PSSnapIn == null && cmdletInfo.Module != null)
                    return cmdletInfo.Module.LogPipelineExecutionDetails;
            // Logging should be enabled for functions from modules also
            FunctionInfo functionInfo = _commandInfo as FunctionInfo;
            if (functionInfo != null && functionInfo.Module != null)
                return functionInfo.Module.LogPipelineExecutionDetails;
        /// This allows all success output to be set to a variable, where the variable is reset for each item returned by
        /// the cmdlet. Semantically this is equivalent to :  cmd | % { $pipelineVariable = $_; (...) }
        internal string PipelineVariable { get; set; }
        private PSVariable _pipelineVarReference;
        private bool _shouldRemovePipelineVariable;
        internal void SetupOutVariable()
            if (string.IsNullOrEmpty(this.OutVariable))
            EnsureVariableParameterAllowed();
            // Handle the creation of OutVariable in the case of Out-Default specially,
            // as it needs to handle much of its OutVariable support itself.
            if (!OutVariable.StartsWith('+') &&
                string.Equals("Out-Default", _commandInfo.Name, StringComparison.OrdinalIgnoreCase))
                _state ??= new SessionState(Context.EngineSessionState);
                IList oldValue = null;
                oldValue = PSObject.Base(_state.PSVariable.GetValue(this.OutVariable)) as IList;
                _outVarList = oldValue ?? new ArrayList();
                if (_thisCommand is not PSScriptCmdlet)
                    this.OutputPipe.AddVariableList(VariableStreamKind.Output, _outVarList);
                _state.PSVariable.Set(this.OutVariable, _outVarList);
                SetupVariable(VariableStreamKind.Output, this.OutVariable, ref _outVarList);
        internal void SetupPipelineVariable()
            // This can't use the common SetupVariable implementation, as this needs to persist for an entire
            // pipeline.
            if (string.IsNullOrEmpty(PipelineVariable))
            // Create the pipeline variable
            _pipelineVarReference = new PSVariable(PipelineVariable);
            object varToUse = _state.Internal.SetVariable(
                _pipelineVarReference,
                force: false,
            if (ReferenceEquals(_pipelineVarReference, varToUse))
                // The returned variable is the exact same instance, which means we set a new variable.
                // In this case, we will try removing the pipeline variable in the end.
                _shouldRemovePipelineVariable = true;
                // A variable with the same name already exists in the same scope and it was returned.
                // In this case, we update the reference and don't remove the variable in the end.
                _pipelineVarReference = (PSVariable)varToUse;
                this.OutputPipe.SetPipelineVariable(_pipelineVarReference);
        internal void RemovePipelineVariable()
            if (_shouldRemovePipelineVariable)
                // Remove pipeline variable when a pipeline is being torn down.
                _state.PSVariable.Remove(PipelineVariable);
        /// Configures the number of objects to buffer before calling the downstream Cmdlet.
        internal int OutBuffer
            get { return OutputPipe.OutBufferCount; }
            set { OutputPipe.OutBufferCount = value; }
        #region ShouldProcess
        /// Confirm the operation with the user.  Cmdlets which make changes
        /// or ActionPreference.Inquire,
        /// <see cref="System.Management.Automation.Cmdlet.ShouldProcess(string)"/>
        /// will throw
        /// <example>
        ///     <code>
        ///         namespace Microsoft.Samples.Cmdlet
        ///         {
        ///             [Cmdlet(VerbsCommon.Remove,"myobjecttype1")]
        ///             public class RemoveMyObjectType1 : PSCmdlet
        ///             {
        ///                 [Parameter( Mandatory = true )]
        ///                 public string Filename
        ///                 {
        ///                     get { return filename; }
        ///                     set { filename = value; }
        ///                 }
        ///                 private string filename;
        ///                 public override void ProcessRecord()
        ///                     if (ShouldProcess(filename))
        ///                     {
        ///                         // delete the object
        ///                     }
        ///             }
        ///         }
        ///     </code>
        /// </example>
        /// <seealso cref="System.Management.Automation.Cmdlet.ShouldProcess(string,string)"/>
        /// <seealso cref="System.Management.Automation.Cmdlet.ShouldProcess(string,string,string)"/>
        /// <seealso cref="System.Management.Automation.Cmdlet.ShouldProcess(string,string,string,out ShouldProcessReason)"/>
        /// <seealso cref="System.Management.Automation.Cmdlet.ShouldContinue(string,string)"/>
        /// <seealso cref="System.Management.Automation.Cmdlet.ShouldContinue(string,string,ref bool,ref bool)"/>
        public bool ShouldProcess(string target)
                MyInvocation.MyCommand.Name,
            return DoShouldProcess(verboseDescription, null, null, out shouldProcessReason);
        ///             [Cmdlet(VerbsCommon.Remove,"myobjecttype2")]
        ///             public class RemoveMyObjectType2 : PSCmdlet
        ///                     if (ShouldProcess(filename, "delete"))
        /// <seealso cref="System.Management.Automation.Cmdlet.ShouldProcess(string)"/>
        public bool ShouldProcess(string target, string action)
        ///             [Cmdlet(VerbsCommon.Remove,"myobjecttype3")]
        ///             public class RemoveMyObjectType3 : PSCmdlet
        ///                     if (ShouldProcess(
        ///                         string.Format($"Deleting file {filename}"),
        ///                         string.Format($"Are you sure you want to delete file {filename}?"),
        ///                         "Delete file"))
        /// <seealso cref="System.Management.Automation.Cmdlet.ShouldProcess(string,string,string, out ShouldProcessReason)"/>
        public bool ShouldProcess(
            return DoShouldProcess(
                verboseDescription,
                verboseWarning,
                out shouldProcessReason);
        ///                     ShouldProcessReason shouldProcessReason;
        ///                         "Delete file",
        ///                         out shouldProcessReason))
            out ShouldProcessReason shouldProcessReason)
        private bool CanShouldProcessAutoConfirm()
            // retrieve ConfirmImpact from commandInfo
            CommandMetadata commandMetadata = _commandInfo.CommandMetadata;
                Dbg.Assert(false, "Expected CommandMetadata");
            ConfirmImpact cmdletConfirmImpact = commandMetadata.ConfirmImpact;
            // compare to ConfirmPreference
            ConfirmImpact threshold = ConfirmPreference;
            if ((threshold == ConfirmImpact.None) || (threshold > cmdletConfirmImpact))
        /// Helper function for ShouldProcess APIs.
        /// Description of operation, to be printed for Continue or WhatIf
        /// Warning prompt, to be printed for Inquire
        /// This is the caption of the window which may be displayed
        /// <remarks>true if-and-only-if the action should be performed</remarks>
        private bool DoShouldProcess(
            shouldProcessReason = ShouldProcessReason.None;
            switch (lastShouldProcessContinueStatus)
                case ContinueStatus.NoToAll:
                case ContinueStatus.YesToAll:
            if (WhatIf)
                // 2005/05/24 908827
                // WriteDebug/WriteVerbose/WriteProgress/WriteWarning should only be callable from the main thread
                shouldProcessReason = ShouldProcessReason.WhatIf;
                string whatIfMessage =
                    StringUtil.Format(CommandBaseStrings.ShouldProcessWhatIfMessage,
                        verboseDescription);
                CBhost.InternalUI.TranscribeResult(whatIfMessage);
                CBhost.UI.WriteLine(whatIfMessage);
            if (this.CanShouldProcessAutoConfirm())
                if (this.Verbose)
                    WriteVerbose(verboseDescription);
            if (string.IsNullOrEmpty(verboseWarning))
                verboseWarning = StringUtil.Format(CommandBaseStrings.ShouldProcessWarningFallback,
            lastShouldProcessContinueStatus = InquireHelper(
                true,   // allowYesToAll
                true,   // allowNoToAll
                false,  // replaceNoWithHalt
                false   // hasSecurityImpact
                case ContinueStatus.No:
        internal enum ShouldProcessPossibleOptimization
            AutoYes_CanSkipShouldProcessCall,
            AutoYes_CanCallShouldProcessAsynchronously,
            AutoNo_CanCallShouldProcessAsynchronously,
            NoOptimizationPossible,
        internal ShouldProcessPossibleOptimization CalculatePossibleShouldProcessOptimization()
            if (this.WhatIf)
                return ShouldProcessPossibleOptimization.AutoNo_CanCallShouldProcessAsynchronously;
                    return ShouldProcessPossibleOptimization.AutoYes_CanCallShouldProcessAsynchronously;
                    return ShouldProcessPossibleOptimization.AutoYes_CanSkipShouldProcessCall;
            return ShouldProcessPossibleOptimization.NoOptimizationPossible;
        #endregion ShouldProcess
        #region ShouldContinue
        ///             [Cmdlet(VerbsCommon.Remove,"myobjecttype4")]
        ///             public class RemoveMyObjectType4 : PSCmdlet
        ///                 [Parameter]
        ///                 public SwitchParameter Force
        ///                     get { return force; }
        ///                     set { force = value; }
        ///                 private bool force;
        ///                         string.Format($"Are you sure you want to delete file {filename}"),
        ///                         if (IsReadOnly(filename))
        ///                         {
        ///                             if (!Force &amp;&amp; !ShouldContinue(
        ///                                     string.Format($"File {filename} is read-only.  Are you sure you want to delete read-only file {filename}?"),
        ///                                     "Delete file"))
        ///                                     )
        ///                             {
        ///                                 return;
        ///                             }
        ///                         }
        public bool ShouldContinue(string query, string caption)
            return DoShouldContinue(
                hasSecurityImpact: false,
                supportsToAllOptions: false,
                ref yesToAll,
                ref noToAll);
        public bool ShouldContinue(
            string query, string caption, bool hasSecurityImpact, ref bool yesToAll, ref bool noToAll)
            return DoShouldContinue(query, caption, hasSecurityImpact, true, ref yesToAll, ref noToAll);
        ///             public class RemoveMyObjectType5 : PSCmdlet
        ///                 private bool yesToAll;
        ///                 private bool noToAll;
        ///                                     "Delete file"),
        ///                                     ref yesToAll,
        ///                                     ref noToAll
            string query, string caption, ref bool yesToAll, ref bool noToAll)
            return DoShouldContinue(query, caption, false, true, ref yesToAll, ref noToAll);
        private bool DoShouldContinue(
            bool hasSecurityImpact,
            bool supportsToAllOptions,
            ref bool yesToAll,
            ref bool noToAll)
            if (noToAll)
            else if (yesToAll)
            ContinueStatus continueStatus = InquireHelper(
                supportsToAllOptions, // allowYesToAll
                supportsToAllOptions, // allowNoToAll
                false,                // replaceNoWithHalt
                hasSecurityImpact     // hasSecurityImpact
            switch (continueStatus)
                    noToAll = true;
                    yesToAll = true;
        #endregion ShouldContinue
        /// Returns true if a transaction is available for use.
        public bool TransactionAvailable()
            return UseTransactionFlagSet && Context.TransactionManager.HasTransaction;
                if (!TransactionAvailable())
                    if (!UseTransactionFlagSet)
                        error = TransactionStrings.CmdletRequiresUseTx;
                        error = TransactionStrings.NoTransactionAvailable;
                return new PSTransactionContext(Context.TransactionManager);
        /// Implementation of ThrowTerminatingError.
        /// always
        /// always throws
        /// regardless of what error was specified in <paramref name="errorRecord"/>.
        /// The Cmdlet should generally just allow
        /// <see cref="System.Management.Automation.PipelineStoppedException"/>.
        /// to percolate up to the caller of
        /// <see cref="System.Management.Automation.Cmdlet.ProcessRecord"/>.
        /// etc.
                throw PSTraceSource.NewArgumentNullException(nameof(errorRecord));
            errorRecord.SetInvocationInfo(MyInvocation);
            if (errorRecord.ErrorDetails != null
                && errorRecord.ErrorDetails.TextLookupError != null)
                Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
                errorRecord.ErrorDetails.TextLookupError = null;
                    textLookupError,
            // This code forces the stack trace and source fields to be populated
            if (errorRecord.Exception != null
                && string.IsNullOrEmpty(errorRecord.Exception.StackTrace))
                    // no need to worry about severe exceptions since
                    // it wasn't really thrown originally
            CmdletInvocationException e =
                new CmdletInvocationException(errorRecord);
            // If the error action preference is set to break, break immediately
            // into the debugger
            if (ErrorAction == ActionPreference.Break)
                Context.Debugger?.Break(e.InnerException ?? e);
            // Code sees only that execution stopped
            throw ManageException(e);
        #endregion Misc
        #region Data Merging
        /// Data streams available for merging.
        internal enum MergeDataStream
            /// No data stream available for merging.
            /// All data streams.
            All = 1,
            /// Success output.
            Output = 2,
            /// Error output.
            Error = 3,
            /// Warning output.
            /// Verbose output.
            Verbose = 5,
            /// Debug output.
            Debug = 6,
            /// Host output.
            Host = 7,
            /// Information output.
            Information = 8
        /// Get/sets error data stream merge state.
        internal MergeDataStream ErrorMergeTo { get; set; }
        /// Method to set data stream merging based on passed in runtime object.
        /// <param name="fromRuntime">MshCommandRuntime object.</param>
        internal void SetMergeFromRuntime(MshCommandRuntime fromRuntime)
            this.ErrorMergeTo = fromRuntime.ErrorMergeTo;
            if (fromRuntime.WarningOutputPipe != null)
                this.WarningOutputPipe = fromRuntime.WarningOutputPipe;
            if (fromRuntime.VerboseOutputPipe != null)
                this.VerboseOutputPipe = fromRuntime.VerboseOutputPipe;
            if (fromRuntime.DebugOutputPipe != null)
                this.DebugOutputPipe = fromRuntime.DebugOutputPipe;
            if (fromRuntime.InformationOutputPipe != null)
                this.InformationOutputPipe = fromRuntime.InformationOutputPipe;
        // Legacy merge hints.
        /// Claims the unclaimed error output of all previous commands.
        internal bool MergeUnclaimedPreviousErrorResults { get; set; } = false;
        #region Internal Pipes
        /// Gets or sets the input pipe.
        internal Pipe InputPipe
            get { return _inputPipe ??= new Pipe(); }
            set { _inputPipe = value; }
        /// Gets or sets the output pipe.
        internal Pipe OutputPipe
            get { return _outputPipe ??= new Pipe(); }
            set { _outputPipe = value; }
        internal object[] GetResultsAsArray()
            if (_outputPipe == null)
                return StaticEmptyArray;
            return _outputPipe.ToArray();
        /// An empty array that is declared statically so we don't keep
        /// allocating them over and over...
        internal static readonly object[] StaticEmptyArray = Array.Empty<object>();
        /// Gets or sets the error pipe.
        internal Pipe ErrorOutputPipe
            get { return _errorOutputPipe ??= new Pipe(); }
            set { _errorOutputPipe = value; }
        /// Gets or sets the warning output pipe.
        internal Pipe WarningOutputPipe { get; set; }
        /// Gets or sets the verbose output pipe.
        internal Pipe VerboseOutputPipe { get; set; }
        /// Gets or sets the debug output pipe.
        internal Pipe DebugOutputPipe { get; set; }
        /// Gets or sets the informational output pipe.
        internal Pipe InformationOutputPipe { get; set; }
        #region Internal helpers
        /// Throws if the caller is trying to call WriteObject/WriteError
        /// from the wrong thread, or not during a call to
        /// BeginProcessing/ProcessRecord/EndProcessing.
        /// <exception cref="System.InvalidOperationException"></exception>
        internal void ThrowIfWriteNotPermitted(bool needsToWriteToPipeline)
            if (this.PipelineProcessor == null
                || _thisCommand != this.PipelineProcessor._permittedToWrite
                || needsToWriteToPipeline && !this.PipelineProcessor._permittedToWriteToPipeline
                || Thread.CurrentThread != this.PipelineProcessor._permittedToWriteThread
                // Only generate these exceptions if a pipeline has already been declared as the 'writing' pipeline.
                // Otherwise, these are probably infrastructure messages and can be ignored.
                if (this.PipelineProcessor?._permittedToWrite != null)
                        PipelineStrings.WriteNotPermitted);
        /// WriteObject/WriteObjecs/WriteError are only allowed during this scope.
        /// Be sure to use this object only in "using" so that it is reliably
        /// disposed and follows stack semantics.
        /// <returns>IDisposable.</returns>
        internal IDisposable AllowThisCommandToWrite(bool permittedToWriteToPipeline)
            return new AllowWrite(_thisCommand, permittedToWriteToPipeline);
        private sealed class AllowWrite : IDisposable
            /// Begin the scope where WriteObject/WriteError is permitted.
            internal AllowWrite(InternalCommand permittedToWrite, bool permittedToWriteToPipeline)
                if (permittedToWrite == null)
                    throw PSTraceSource.NewArgumentNullException(nameof(permittedToWrite));
                if (permittedToWrite.commandRuntime is not MshCommandRuntime mcr)
                    throw PSTraceSource.NewArgumentNullException("permittedToWrite.CommandRuntime");
                _pp = mcr.PipelineProcessor;
                    throw PSTraceSource.NewArgumentNullException("permittedToWrite.CommandRuntime.PipelineProcessor");
                _wasPermittedToWrite = _pp._permittedToWrite;
                _wasPermittedToWriteToPipeline = _pp._permittedToWriteToPipeline;
                _wasPermittedToWriteThread = _pp._permittedToWriteThread;
                _pp._permittedToWrite = permittedToWrite;
                _pp._permittedToWriteToPipeline = permittedToWriteToPipeline;
                _pp._permittedToWriteThread = Thread.CurrentThread;
            /// End the scope where WriteObject/WriteError is permitted.
                _pp._permittedToWrite = _wasPermittedToWrite;
                _pp._permittedToWriteToPipeline = _wasPermittedToWriteToPipeline;
                _pp._permittedToWriteThread = _wasPermittedToWriteThread;
            // There is no finalizer, by design.  This class relies on always
            // being disposed and always following stack semantics.
            private readonly PipelineProcessor _pp = null;
            private readonly InternalCommand _wasPermittedToWrite = null;
            private readonly bool _wasPermittedToWriteToPipeline = false;
            private readonly Thread _wasPermittedToWriteThread = null;
        /// PipelineProcessor.SynchronousExecute,
        /// and writes it to the error variable.
        /// The general pattern is to call
        /// throw ManageException(e);
        /// <param name="e">The exception.</param>
        /// <returns>PipelineStoppedException.</returns>
        public Exception ManageException(Exception e)
                throw PSTraceSource.NewArgumentNullException(nameof(e));
            PipelineProcessor?.RecordFailure(e, _thisCommand);
            // 913088-2005/06/06
            // PipelineStoppedException should not get added to $Error
            // 2008/06/25 - narrieta: ExistNestedPromptException should not be added to $error either
            // 2019/10/18 - StopUpstreamCommandsException should not be added either
            if (e is not HaltCommandException
                && e is not PipelineStoppedException
                && e is not ExitNestedPromptException
                && e is not StopUpstreamCommandsException)
                    AppendErrorToVariables(e);
                    // Catch all OK, the error variables might be corrupted.
            // Upstream Cmdlets see only that execution stopped
        #endregion Internal helpers
        #region Error PSVariable
        private IList _errorVarList;
        /// ErrorVariable tells which variable to populate with the errors.
        internal string ErrorVariable { get; set; }
        internal void SetupErrorVariable()
            SetupVariable(VariableStreamKind.Error, this.ErrorVariable, ref _errorVarList);
        private void EnsureVariableParameterAllowed()
            if ((Context.LanguageMode == PSLanguageMode.NoLanguage) ||
                (Context.LanguageMode == PSLanguageMode.RestrictedLanguage))
                    null, "VariableReferenceNotSupportedInDataSection",
                    ParserStrings.VariableReferenceNotSupportedInDataSection,
                    ParserStrings.DefaultAllowedVariablesInDataSection);
        /// Append an error to the ErrorVariable if specified, and also to $ERROR.
        /// <param name="obj">Exception or ErrorRecord.</param>
        /// <exception cref="System.Management.Automation.ExtendedTypeSystemException">
        /// (An error occurred working with the error variable or $ERROR.
        internal void AppendErrorToVariables(object obj)
            AppendDollarError(obj);
            this.OutputPipe.AppendVariableList(VariableStreamKind.Error, obj);
        /// Appends the object to $global:error.  Non-terminating errors
        /// are always added (even if they are redirected to another
        /// Cmdlet), but terminating errors are only added if they are
        /// at the top-level scope (the LocalPipeline scope).
        /// We insert at position 0 and delete from position 63.
        /// An error occurred accessing $ERROR.
        private void AppendDollarError(object obj)
            if (obj is Exception)
                if (this.PipelineProcessor == null || !this.PipelineProcessor.TopLevel)
                    return; // not outermost scope
            Context.AppendDollarError(obj);
        #endregion Error PSVariable
        #region Warning PSVariable
        private IList _warningVarList;
        /// WarningVariable tells which variable to populate with the warnings.
        internal string WarningVariable { get; set; }
        internal void SetupWarningVariable()
            SetupVariable(VariableStreamKind.Warning, this.WarningVariable, ref _warningVarList);
        /// Append a warning to WarningVariable if specified.
        /// <param name="obj">The warning message.</param>
        internal void AppendWarningVarList(object obj)
            this.OutputPipe.AppendVariableList(VariableStreamKind.Warning, obj);
        #endregion Warning PSVariable
        #region Information PSVariable
        private IList _informationVarList;
        /// InformationVariable tells which variable to populate with informational output.
        internal string InformationVariable { get; set; }
        internal void SetupInformationVariable()
            SetupVariable(VariableStreamKind.Information, this.InformationVariable, ref _informationVarList);
        internal void SetupVariable(VariableStreamKind streamKind, string variableName, ref IList varList)
            if (string.IsNullOrEmpty(variableName))
            if (variableName.StartsWith('+'))
                variableName = variableName.Substring(1);
                object oldValue = PSObject.Base(_state.PSVariable.GetValue(variableName));
                varList = oldValue as IList;
                if (varList == null)
                    varList = new ArrayList();
                    if (oldValue != null && AutomationNull.Value != oldValue)
                        IEnumerable enumerable = LanguagePrimitives.GetEnumerable(oldValue);
                                varList.Add(o);
                            varList.Add(oldValue);
                else if (varList.IsFixedSize)
                    ArrayList varListNew = new ArrayList();
                    varListNew.AddRange(varList);
                    varList = varListNew;
                this.OutputPipe.AddVariableList(streamKind, varList);
            _state.PSVariable.Set(variableName, varList);
        /// Append a Information to InformationVariable if specified.
        /// <param name="obj">The Information message.</param>
        internal void AppendInformationVarList(object obj)
            this.OutputPipe.AppendVariableList(VariableStreamKind.Information, obj);
        #endregion Information PSVariable
        internal bool UseSecurityContextRun = true;
        /// Writes an object to the output pipe, skipping the ThrowIfWriteNotPermitted check.
        /// The object to write to the output pipe.
        internal void _WriteObjectSkipAllowCheck(object sendToPipeline)
            if (AutomationNull.Value == sendToPipeline)
            sendToPipeline = LanguagePrimitives.AsPSObjectOrNull(sendToPipeline);
            this.OutputPipe.Add(sendToPipeline);
        /// Enumerates and writes an object to the output pipe, skipping the ThrowIfWriteNotPermitted check.
        /// The object to enumerate and write to the output pipe.
        internal void _EnumerateAndWriteObjectSkipAllowCheck(object sendToPipeline)
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(sendToPipeline);
            if (enumerable == null)
            ArrayList convertedList = new ArrayList();
            foreach (object toConvert in enumerable)
                if (AutomationNull.Value == toConvert)
                object converted = LanguagePrimitives.AsPSObjectOrNull(toConvert);
                convertedList.Add(converted);
            // Writing normal output with "2>&1"
            // bypasses ErrorActionPreference, as intended.
            this.OutputPipe.AddItems(convertedList);
        #region WriteError
            WriteError(errorRecord, false);
        internal void WriteError(ErrorRecord errorRecord, bool overrideInquire)
            // This check will be repeated in _WriteErrorSkipAllowCheck,
            ActionPreference preference = ErrorAction;
                CBhost?.Runspace?.Debugger?.Break(errorRecord);
            DoWriteError(new KeyValuePair<ErrorRecord, ActionPreference>(errorRecord, preference));
                    new ContextCallback(DoWriteError);
                    new KeyValuePair<ErrorRecord, ActionPreference>(errorRecord, preference));
        private void DoWriteError(object obj)
            KeyValuePair<ErrorRecord, ActionPreference> pair = (KeyValuePair<ErrorRecord, ActionPreference>)obj;
            ErrorRecord errorRecord = pair.Key;
            ActionPreference preference = pair.Value;
                throw PSTraceSource.NewArgumentNullException("errorRecord");
            // If this error came from a transacted cmdlet,
            if (UseTransaction)
                   (Context.TransactionManager.RollbackPreference != RollbackSeverity.TerminatingError) &&
                   (Context.TransactionManager.RollbackPreference != RollbackSeverity.Never))
            if (errorRecord.PreserveInvocationInfoOnce)
                errorRecord.PreserveInvocationInfoOnce = false;
            _WriteErrorSkipAllowCheck(errorRecord, preference);
        /// Write an error, skipping the ThrowIfWriteNotPermitted check.
        /// <param name="errorRecord">The error record to write.</param>
        /// <param name="actionPreference">The configured error action preference.</param>
        /// <param name="isFromNativeStdError">
        /// True when this method is called to write from a native command's stderr stream.
        /// When errors are written through a native stderr stream, they do not interact with the error preference system,
        /// but must still present as errors in PowerShell.
        internal void _WriteErrorSkipAllowCheck(ErrorRecord errorRecord, ActionPreference? actionPreference = null, bool isFromNativeStdError = false)
            if (LogPipelineExecutionDetail)
                this.PipelineProcessor.LogExecutionError(_thisCommand.MyInvocation, errorRecord);
            if (!isFromNativeStdError)
                this.PipelineProcessor.ExecutionFailed = true;
                if (actionPreference.HasValue)
                    preference = actionPreference.Value;
                // No trace of the error in the 'Ignore' case
                if (preference == ActionPreference.Ignore)
                    return; // do not write or record to output pipe
                // 2004/05/26-JonN
                // The object is not written in the SilentlyContinue case
                if (preference == ActionPreference.SilentlyContinue)
                    AppendErrorToVariables(errorRecord);
                    return; // do not write to output pipe
                if (lastErrorContinueStatus == ContinueStatus.YesToAll)
                switch (preference)
                        ActionPreferenceStopException e =
                            new ActionPreferenceStopException(
                                MyInvocation,
                                errorRecord,
                                StringUtil.Format(CommandBaseStrings.ErrorPreferenceStop,
                                                "ErrorActionPreference",
                                                errorRecord.ToString()));
                        // ignore return value
                        // this will throw if the user chooses not to continue
                        lastErrorContinueStatus = InquireHelper(
                            RuntimeException.RetrieveMessage(errorRecord),
                            true,  // allowYesToAll
                            false, // allowNoToAll
                            true,  // replaceNoWithHalt
                            false  // hasSecurityImpact
            // Add this note property and set its value to true for F&O
            // to decide whether to call WriteErrorLine or WriteLine.
            // We want errors to print in red in both cases.
            PSObject errorWrap = PSObject.AsPSObject(errorRecord);
            // It's possible we've already added the member (this method is recursive sometimes
            // when tracing), so don't add the member again.
            // We don't add a note property on messages that comes from stderr stream.
            // 2003/11/19-JonN Previously, PSObject instances in ErrorOutputPipe
            // wrapped the TargetObject and held the CoreException as a note.
            // Now, they wrap the CoreException and hold the TargetObject as a note.
            if (ErrorMergeTo != MergeDataStream.None)
                Dbg.Assert(ErrorMergeTo == MergeDataStream.Output, "Only merging to success output is supported.");
                this.OutputPipe.AddWithoutAppendingOutVarList(errorWrap);
                // If this is an error pipe for a hosting application and we are logging,
                // then create a temporary PowerShell to log the error.
                if (Context.InternalHost.UI.IsTranscribing)
                    Context.InternalHost.UI.TranscribeError(Context, errorRecord.InvocationInfo, errorWrap);
                this.ErrorOutputPipe.AddWithoutAppendingOutVarList(errorWrap);
        #endregion WriteError
        #region Preference
        // These are a set of preference variables which affect the inner
        // workings of the command and when what information will get output.
        // See "User Feedback Mechanisms - Note.doc" for details.
        private bool _isConfirmPreferenceCached = false;
        private ConfirmImpact _confirmPreference = InitialSessionState.DefaultConfirmPreference;
        /// Preference setting controlling behavior of ShouldProcess()
        /// This is not an independent parameter, it just emerges from the
        /// Verbose, Debug, Confirm, and WhatIf parameters and the
        /// $ConfirmPreference shell variable.
        /// We only read $ConfirmPreference once, then cache the value.
        internal ConfirmImpact ConfirmPreference
                // WhatIf not relevant, it never gets this far in that case
                if (Confirm)
                    return ConfirmImpact.Low;
                if (IsConfirmFlagSet)
                    // -Confirm:$false
                    return ConfirmImpact.None;
                if (!_isConfirmPreferenceCached)
                    _confirmPreference = Context.GetEnumPreference(SpecialVariables.ConfirmPreferenceVarPath, _confirmPreference, out _);
                    _isConfirmPreferenceCached = true;
                return _confirmPreference;
        private bool _isDebugPreferenceSet = false;
        private ActionPreference _debugPreference = InitialSessionState.DefaultDebugPreference;
        private bool _isDebugPreferenceCached = false;
        /// Preference setting.
        /// (get-only) An error occurred accessing $DebugPreference.
        internal ActionPreference DebugPreference
                if (_isDebugPreferenceSet)
                    return _debugPreference;
                if (IsDebugFlagSet)
                    return Debug ? ActionPreference.Continue : ActionPreference.SilentlyContinue;
                if (!_isDebugPreferenceCached)
                    _debugPreference = Context.GetEnumPreference(SpecialVariables.DebugPreferenceVarPath, _debugPreference, out defaultUsed);
                    // If the host couldn't prompt for the debug action anyways, change it to 'Continue'.
                    // This lets hosts still see debug output without having to implement the prompting logic.
                    if ((CBhost.ExternalHost.UI == null) && (_debugPreference == ActionPreference.Inquire))
                        _debugPreference = ActionPreference.Continue;
                    _isDebugPreferenceCached = true;
                if (value == ActionPreference.Suspend)
                    throw PSTraceSource.NewNotSupportedException(ErrorPackage.ActionPreferenceReservedForFutureUseError, value);
                _debugPreference = value;
                _isDebugPreferenceSet = true;
        private readonly bool _isVerbosePreferenceCached = false;
        private ActionPreference _verbosePreference = InitialSessionState.DefaultVerbosePreference;
        /// An error occurred accessing $VerbosePreference.
        internal ActionPreference VerbosePreference
                if (IsVerboseFlagSet)
                    if (Verbose)
                        return ActionPreference.Continue;
                        return ActionPreference.SilentlyContinue;
                if (Debug)
                    // If the host couldn't prompt for the debug action anyways, use 'Continue'.
                    if (CBhost.ExternalHost.UI == null)
                        return ActionPreference.Inquire;
                if (!_isVerbosePreferenceCached)
                    _verbosePreference = Context.GetEnumPreference(
                        _verbosePreference,
                return _verbosePreference;
        internal bool IsWarningActionSet { get; private set; } = false;
        private readonly bool _isWarningPreferenceCached = false;
        private ActionPreference _warningPreference = InitialSessionState.DefaultWarningPreference;
        /// An error occurred accessing $WarningPreference.
        internal ActionPreference WarningPreference
                // Setting CommonParameters.WarningAction has highest priority
                if (IsWarningActionSet)
                    return _warningPreference;
                // Debug:$false and Verbose:$false ignored
                if (!_isWarningPreferenceCached)
                    _warningPreference = Context.GetEnumPreference(SpecialVariables.WarningPreferenceVarPath, _warningPreference, out defaultUsed);
                _warningPreference = value;
                IsWarningActionSet = true;
        // This is used so that people can tell whether the verbose switch
        // was specified.  This is useful in the Cmdlet-calling-Cmdlet case
        // where you'd like the underlying Cmdlet to have the same switches.
        private bool _verboseFlag = false;
        /// Echo tells the command to articulate the actions it performs while executing.
        internal bool Verbose
                return _verboseFlag;
                _verboseFlag = value;
                IsVerboseFlagSet = true;
        internal bool IsVerboseFlagSet { get; private set; } = false;
        private bool _confirmFlag = false;
        /// Confirm tells the command to ask the admin before performing an action.
        /// This is a common parameter via class ShouldProcessParameters.
        internal SwitchParameter Confirm
                return _confirmFlag;
                _confirmFlag = value;
                IsConfirmFlagSet = true;
        internal bool IsConfirmFlagSet { get; private set; } = false;
        private bool _useTransactionFlag = false;
        /// UseTransaction tells the command to activate the current PowerShell transaction.
        /// This is a common parameter via class TransactionParameters.
        internal SwitchParameter UseTransaction
                return _useTransactionFlag;
                _useTransactionFlag = value;
                UseTransactionFlagSet = true;
        internal bool UseTransactionFlagSet { get; private set; } = false;
        // This is used so that people can tell whether the debug switch was specified.  This
        // Is useful in the Cmdlet-calling-Cmdlet case where you'd like the underlying Cmdlet to
        // have the same switches.
        private bool _debugFlag = false;
        /// Debug tell the command system to provide Programmer/Support type messages to understand what is really occurring
        /// and give the user the opportunity to stop or debug the situation.
        internal bool Debug
                return _debugFlag;
                _debugFlag = value;
                IsDebugFlagSet = true;
        internal bool IsDebugFlagSet { get; private set; } = false;
        private bool _whatIfFlag = InitialSessionState.DefaultWhatIfPreference;
        private bool _isWhatIfPreferenceCached /* = false */;
        /// WhatIf indicates that the command should not
        /// perform any changes to persistent state outside Monad.
        internal SwitchParameter WhatIf
                if (!IsWhatIfFlagSet && !_isWhatIfPreferenceCached)
                    _whatIfFlag = Context.GetBooleanPreference(SpecialVariables.WhatIfPreferenceVarPath, _whatIfFlag, out _);
                    _isWhatIfPreferenceCached = true;
                return _whatIfFlag;
                _whatIfFlag = value;
                IsWhatIfFlagSet = true;
        internal bool IsWhatIfFlagSet { get; private set; }
        private ActionPreference _errorAction = InitialSessionState.DefaultErrorActionPreference;
        private bool _isErrorActionPreferenceCached = false;
        /// ErrorAction tells the command what to do when an error occurs.
        /// (get-only) An error occurred accessing $ErrorAction.
        internal ActionPreference ErrorAction
                // Setting CommonParameters.ErrorAction has highest priority
                if (IsErrorActionSet)
                    return _errorAction;
                if (!_isErrorActionPreferenceCached)
                    _errorAction = Context.GetEnumPreference(SpecialVariables.ErrorActionPreferenceVarPath, _errorAction, out defaultUsed);
                    _isErrorActionPreferenceCached = true;
                _errorAction = value;
                IsErrorActionSet = true;
        internal bool IsErrorActionSet { get; private set; } = false;
        /// Preference setting for displaying ProgressRecords when WriteProgress is called.
        internal ActionPreference ProgressPreference
                if (IsProgressActionSet)
                    return _progressPreference;
                if (!_isProgressPreferenceCached)
                    _progressPreference = Context.GetEnumPreference(SpecialVariables.ProgressPreferenceVarPath, _progressPreference, out defaultUsed);
                    _isProgressPreferenceCached = true;
                _progressPreference = value;
                IsProgressActionSet = true;
        private ActionPreference _progressPreference = InitialSessionState.DefaultProgressPreference;
        internal bool IsProgressActionSet { get; private set; } = false;
        private bool _isProgressPreferenceCached = false;
        /// Preference setting for displaying InformationRecords when WriteInformation is called.
        internal ActionPreference InformationPreference
                if (IsInformationActionSet)
                    return _informationPreference;
                if (!_isInformationPreferenceCached)
                    _informationPreference = Context.GetEnumPreference(SpecialVariables.InformationPreferenceVarPath, _informationPreference, out defaultUsed);
                    _isInformationPreferenceCached = true;
                _informationPreference = value;
                IsInformationActionSet = true;
        private ActionPreference _informationPreference = InitialSessionState.DefaultInformationPreference;
        internal bool IsInformationActionSet { get; private set; } = false;
        private bool _isInformationPreferenceCached = false;
        internal PagingParameters PagingParameters { get; set; }
        #endregion Preference
        #region Continue/Confirm
        /// ContinueStatus indicates the last reply from the user
        /// whether or not the command should process an object.
        internal enum ContinueStatus
            No,
            YesToAll,
            NoToAll
        internal ContinueStatus lastShouldProcessContinueStatus = ContinueStatus.Yes;
        internal ContinueStatus lastErrorContinueStatus = ContinueStatus.Yes;
        internal ContinueStatus lastDebugContinueStatus = ContinueStatus.Yes;
        internal ContinueStatus lastVerboseContinueStatus = ContinueStatus.Yes;
        internal ContinueStatus lastWarningContinueStatus = ContinueStatus.Yes;
        internal ContinueStatus lastProgressContinueStatus = ContinueStatus.Yes;
        internal ContinueStatus lastInformationContinueStatus = ContinueStatus.Yes;
        /// Should the verbose/debug/progress message be printed?
        /// <param name="preference"></param>
        /// <param name="lastContinueStatus"></param>
        internal bool WriteHelper_ShouldWrite(
            ActionPreference preference,
            ContinueStatus lastContinueStatus)
            switch (lastContinueStatus)
                case ContinueStatus.NoToAll:  // previously answered NoToAll
                case ContinueStatus.YesToAll: // previously answered YesToAll
                case ActionPreference.Break:
                    Dbg.Assert(false, "Bad preference value" + preference);
        /// Complete implementation of WriteDebug/WriteVerbose/WriteProgress.
        /// <param name="inquireCaption"></param>
        /// <param name="inquireMessage"></param>
        /// <param name="preferenceVariableName"></param>
        /// <returns>Did Inquire return YesToAll?.</returns>
        internal ContinueStatus WriteHelper(
            string inquireCaption,
            string inquireMessage,
            ContinueStatus lastContinueStatus,
            string preferenceVariableName,
                    return ContinueStatus.NoToAll;
                    return ContinueStatus.YesToAll;
                case ActionPreference.Ignore: // YesToAll
                    return ContinueStatus.Yes;
                            StringUtil.Format(CommandBaseStrings.ErrorPreferenceStop, preferenceVariableName, message));
                    ActionPreferenceStopException apse =
                            StringUtil.Format(CommandBaseStrings.PreferenceInvalid, preferenceVariableName, preference));
                    throw ManageException(apse);
            return InquireHelper(
                inquireMessage,
                inquireCaption,
        /// Helper for continue prompt, handles Inquire.
        /// <param name="inquireMessage">May be null.</param>
        /// <param name="inquireCaption">May be null.</param>
        /// <param name="allowYesToAll"></param>
        /// <param name="allowNoToAll"></param>
        /// <param name="replaceNoWithHalt"></param>
        /// <param name="hasSecurityImpact"></param>
        /// <returns>User's selection.</returns>
        internal ContinueStatus InquireHelper(
            bool allowYesToAll,
            bool allowNoToAll,
            bool replaceNoWithHalt,
            bool hasSecurityImpact
            Collection<ChoiceDescription> choices =
                new Collection<ChoiceDescription>();
            int currentOption = 0;
            int continueOneOption = Int32.MaxValue,
                continueAllOption = Int32.MaxValue,
                haltOption = Int32.MaxValue,
                skipOneOption = Int32.MaxValue,
                skipAllOption = Int32.MaxValue,
                pauseOption = Int32.MaxValue;
            string continueOneLabel = CommandBaseStrings.ContinueOneLabel;
            string continueOneHelpMsg = CommandBaseStrings.ContinueOneHelpMessage;
            choices.Add(new ChoiceDescription(continueOneLabel, continueOneHelpMsg));
            continueOneOption = currentOption++;
            if (allowYesToAll)
                string continueAllLabel = CommandBaseStrings.ContinueAllLabel;
                string continueAllHelpMsg = CommandBaseStrings.ContinueAllHelpMessage;
                choices.Add(new ChoiceDescription(continueAllLabel, continueAllHelpMsg));
                continueAllOption = currentOption++;
            if (replaceNoWithHalt)
                string haltLabel = CommandBaseStrings.HaltLabel;
                string haltHelpMsg = CommandBaseStrings.HaltHelpMessage;
                choices.Add(new ChoiceDescription(haltLabel, haltHelpMsg));
                haltOption = currentOption++;
                string skipOneLabel = CommandBaseStrings.SkipOneLabel;
                string skipOneHelpMsg = CommandBaseStrings.SkipOneHelpMessage;
                choices.Add(new ChoiceDescription(skipOneLabel, skipOneHelpMsg));
                skipOneOption = currentOption++;
            if (allowNoToAll)
                string skipAllLabel = CommandBaseStrings.SkipAllLabel;
                string skipAllHelpMsg = CommandBaseStrings.SkipAllHelpMessage;
                choices.Add(new ChoiceDescription(skipAllLabel, skipAllHelpMsg));
                skipAllOption = currentOption++;
            // Hide the "Suspend" option in the remoting case since that is not supported. If the user chooses
            // Suspend that will produce an error message. Why show the user an option that the user cannot use?
            // Related to bug Win7/116823.
            if (IsSuspendPromptAllowed())
                string pauseLabel = CommandBaseStrings.PauseLabel;
                string pauseHelpMsg = StringUtil.Format(CommandBaseStrings.PauseHelpMessage, "exit");
                choices.Add(new ChoiceDescription(pauseLabel, pauseHelpMsg));
                pauseOption = currentOption++;
            if (string.IsNullOrEmpty(inquireMessage))
                inquireMessage = CommandBaseStrings.ShouldContinuePromptCaption;
            if (string.IsNullOrEmpty(inquireCaption))
                inquireCaption = CommandBaseStrings.InquireCaptionDefault;
                // Transcribe the confirmation message
                CBhost.InternalUI.TranscribeResult(inquireCaption);
                CBhost.InternalUI.TranscribeResult(inquireMessage);
                Text.StringBuilder textChoices = new Text.StringBuilder();
                foreach (ChoiceDescription choice in choices)
                    if (textChoices.Length > 0)
                        textChoices.Append("  ");
                    textChoices.Append(choice.Label);
                CBhost.InternalUI.TranscribeResult(textChoices.ToString());
                int defaultOption = 0;
                if (hasSecurityImpact)
                    defaultOption = skipOneOption;
                int response = this.CBhost.UI.PromptForChoice(
                    inquireCaption, inquireMessage, choices, defaultOption);
                string chosen = choices[response].Label;
                int labelIndex = chosen.IndexOf('&');
                if (labelIndex > -1)
                    chosen = chosen[labelIndex + 1].ToString();
                CBhost.InternalUI.TranscribeResult(chosen);
                if (continueOneOption == response)
                else if (continueAllOption == response)
                else if (haltOption == response)
                            CommandBaseStrings.InquireHalt);
                else if (skipOneOption == response)
                    return ContinueStatus.No;
                else if (skipAllOption == response)
                else if (pauseOption == response)
                    // This call returns when the user exits the nested prompt.
                    CBhost.EnterNestedPrompt(_thisCommand);
                    // continue loop
                else if (response == -1)
                            CommandBaseStrings.InquireCtrlC);
                    Dbg.Assert(false, "all cases should be checked");
                    InvalidOperationException e =
        /// Determines if this is being run in the context of a remote host or not.
        private bool IsSuspendPromptAllowed()
            Dbg.Assert(this.CBhost != null, "Expected this.CBhost != null");
            Dbg.Assert(this.CBhost.ExternalHost != null, "Expected this.CBhost.ExternalHost != null");
            if (this.CBhost.ExternalHost is ServerRemoteHost)
        #endregion Continue/Confirm
        internal void SetVariableListsInPipe()
            Diagnostics.Assert(_thisCommand is PSScriptCmdlet, "this is only done for script cmdlets");
            if (_outVarList != null && !OutputPipe.IgnoreOutVariableList)
                // A null pipe is used when executing the 'Clean' block of a PSScriptCmdlet.
                // In such a case, we don't capture output to the out variable list.
            if (_errorVarList != null)
                this.OutputPipe.AddVariableList(VariableStreamKind.Error, _errorVarList);
            if (_warningVarList != null)
                this.OutputPipe.AddVariableList(VariableStreamKind.Warning, _warningVarList);
            if (_informationVarList != null)
                this.OutputPipe.AddVariableList(VariableStreamKind.Information, _informationVarList);
            if (this.PipelineVariable != null)
        internal void RemoveVariableListsInPipe()
                this.OutputPipe.RemoveVariableList(VariableStreamKind.Output, _outVarList);
                this.OutputPipe.RemoveVariableList(VariableStreamKind.Error, _errorVarList);
                this.OutputPipe.RemoveVariableList(VariableStreamKind.Warning, _warningVarList);
                this.OutputPipe.RemoveVariableList(VariableStreamKind.Information, _informationVarList);
                this.OutputPipe.RemovePipelineVariable();

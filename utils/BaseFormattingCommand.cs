    /// Base class defining the formatting context and the
    /// formatting context manager (stack based)
    internal class InnerFormatShapeCommandBase : ImplementationCommandBase
        /// Constructor to set up the formatting context.
        internal InnerFormatShapeCommandBase()
            contextManager.Push(FormattingContextState.none);
        /// Enum listing the possible states the context is in.
        internal enum FormattingContextState { none, document, group }
        /// Context manager: stack to keep track in which context
        /// the formatter is.
        protected Stack<FormattingContextState> contextManager = new Stack<FormattingContextState>();
    /// Core inner implementation for format/xxx commands.
    internal class InnerFormatShapeCommand : InnerFormatShapeCommandBase
        /// Constructor to glue to the CRO.
        internal InnerFormatShapeCommand(FormatShape shape)
            _shape = shape;
        internal static int FormatEnumerationLimit()
            object enumLimitVal = null;
                // Win8: 192504
                if (LocalPipeline.GetExecutionContextFromTLS() != null)
                    enumLimitVal = LocalPipeline.GetExecutionContextFromTLS()
                        .SessionState.PSVariable
                        .GetValue("global:" + InitialSessionState.FormatEnumerationLimit);
            // Eat the following exceptions, enumerationLimit will use the default value
            catch (ProviderInvocationException)
            // if $global:FormatEnumerationLimit is an int, overwrite the default value
            return enumLimitVal is int ? (int)enumLimitVal : InitialSessionState.DefaultFormatEnumerationLimit;
        internal override void BeginProcessing()
            // Get the Format Enumeration Limit.
            _enumerationLimit = InnerFormatShapeCommand.FormatEnumerationLimit();
            _formatObjectDeserializer = new FormatObjectDeserializer(this.TerminatingErrorContext);
        internal override void ProcessRecord()
            _typeInfoDataBase = this.OuterCmdlet().Context.FormatDBManager.GetTypeInfoDataBase();
            PSObject so = this.ReadObject();
            IEnumerable e = PSObjectHelper.GetEnumerable(so);
            if (e == null)
                ProcessObject(so);
            // we have an IEnumerable, we have to decide if to expand, if at all
            EnumerableExpansion expansionState = this.GetExpansionState(so);
            switch (expansionState)
                case EnumerableExpansion.EnumOnly:
                        foreach (object obj in e)
                            ProcessObject(PSObjectHelper.AsPSObject(obj));
                case EnumerableExpansion.Both:
                        var objs = e.Cast<object>().ToArray();
                        ProcessCoreOutOfBand(so, objs.Length);
                        foreach (object obj in objs)
                        // do not enumerate at all (CoreOnly)
        private EnumerableExpansion GetExpansionState(PSObject so)
            // if the command line swtich has been specified, use this as an override
            if (_parameters != null && _parameters.expansion.HasValue)
                return _parameters.expansion.Value;
            // check if we have an expansion entry in format.mshxml
            return DisplayDataQuery.GetEnumerableExpansionFromType(
                _expressionFactory, _typeInfoDataBase, typeNames);
        private void ProcessCoreOutOfBand(PSObject so, int count)
            // emit some description header
            SendCommentOutOfBand(FormatAndOut_format_xxx.IEnum_Header);
            // emit the object as out of band
            ProcessOutOfBand(so, isProcessingError: false);
            // emit a comment to signal that the next N objects are from the IEnumerable
            switch (count)
                        msg = FormatAndOut_format_xxx.IEnum_NoObjects;
                        msg = FormatAndOut_format_xxx.IEnum_OneObject;
                        msg = StringUtil.Format(FormatAndOut_format_xxx.IEnum_ManyObjects, count);
            SendCommentOutOfBand(msg);
        private void SendCommentOutOfBand(string msg)
            FormatEntryData fed = OutOfBandFormatViewManager.GenerateOutOfBandObjectAsToString(PSObjectHelper.AsPSObject(msg));
            if (fed != null)
                this.WriteObject(fed);
        /// <param name="so">Object to process.</param>
        private void ProcessObject(PSObject so)
            // we do protect against reentrancy, assuming
            // no fancy multiplexing
            if (_formatObjectDeserializer.IsFormatInfoData(so))
                // we are already formatted...
                this.WriteObject(so);
            // if objects have to be treated as out of band, just
            // bail now
            // this is the case of objects coming before the
            // context manager is properly set
            if (ProcessOutOfBandObjectOutsideDocumentSequence(so))
            // if we haven't started yet, need to do so
            FormattingContextState ctx = contextManager.Peek();
            if (ctx == FormattingContextState.none)
                // initialize the view manager
                _viewManager.Initialize(this.TerminatingErrorContext, _expressionFactory, _typeInfoDataBase, so, _shape, _parameters);
                // add the start message to output queue
                WriteFormatStartData(so);
                // enter the document context
                contextManager.Push(FormattingContextState.document);
            // if we are here, we are either in the document document, or in a group
            // since we have a view now, we check if objects should be treated as out of band
            if (ProcessOutOfBandObjectInsideDocumentSequence(so))
            // check if we have to enter or exit a group
            GroupTransition transition = ComputeGroupTransition(so);
            if (transition == GroupTransition.enter)
                // insert the group start marker
                PushGroup(so);
                this.WritePayloadObject(so);
            else if (transition == GroupTransition.exit)
                // insert the group end marker
                PopGroup();
            else if (transition == GroupTransition.startNew)
                // Add newline before each group except first
                WriteNewLineObject();
                // double transition
                PopGroup(); // exit the current one
                PushGroup(so); // start a sibling group
            else // none, we did not have any transitions, just push out the data
        private void WriteNewLineObject()
            FormatEntryData fed = new FormatEntryData();
            fed.outOfBand = true;
            ComplexViewEntry cve = new ComplexViewEntry();
            FormatEntry fe = new FormatEntry();
            cve.formatValueList.Add(fe);
            // Formating system writes newline before each object
            // so no need to add newline here like:
            //     fe.formatValueList.Add(new FormatNewLine());
            fed.formatEntryInfo = cve;
        private bool ShouldProcessOutOfBand
                if (_shape == FormatShape.Undefined || _parameters == null)
                return !_parameters.forceFormattingAlsoOnOutOfBand;
        private bool ProcessOutOfBandObjectOutsideDocumentSequence(PSObject so)
            if (!ShouldProcessOutOfBand)
            if (so.InternalTypeNames.Count == 0)
            List<ErrorRecord> errors;
            var fed = OutOfBandFormatViewManager.GenerateOutOfBandData(this.TerminatingErrorContext, _expressionFactory,
                _typeInfoDataBase, so, _enumerationLimit, false, out errors);
            WriteErrorRecords(errors);
        private bool ProcessOutOfBandObjectInsideDocumentSequence(PSObject so)
            if (_viewManager.ViewGenerator.IsObjectApplicable(typeNames))
            return ProcessOutOfBand(so, isProcessingError: false);
        private bool ProcessOutOfBand(PSObject so, bool isProcessingError)
            FormatEntryData fed = OutOfBandFormatViewManager.GenerateOutOfBandData(this.TerminatingErrorContext, _expressionFactory,
                                    _typeInfoDataBase, so, _enumerationLimit, true, out errors);
            if (!isProcessingError)
        protected void WriteInternalErrorMessage(string message)
            fe.formatValueList.Add(new FormatNewLine());
            // get a field for the message
            FormatTextField ftf = new FormatTextField();
            ftf.text = message;
            fe.formatValueList.Add(ftf);
        private void WriteErrorRecords(List<ErrorRecord> errorRecordList)
            if (errorRecordList == null)
            // NOTE: for the time being we directly process error records.
            // This is should change if we hook up error pipelines; for the
            // time being, this achieves partial results.
            // see NTRAID#Windows OS Bug-932722-2004/10/21-kevinloo ("Output: SS: Swallowing exceptions")
            foreach (ErrorRecord errorRecord in errorRecordList)
                // we are recursing on formatting errors: isProcessingError == true
                ProcessOutOfBand(PSObjectHelper.AsPSObject(errorRecord), true);
        internal override void EndProcessing()
            // need to pop all the contexts, in case the transmission sequence
            // was interrupted
                    break; // we emerged and we are done
                else if (ctx == FormattingContextState.group)
                else if (ctx == FormattingContextState.document)
                    // inject the end format information
                    FormatEndData endFormat = new FormatEndData();
                    this.WriteObject(endFormat);
                    contextManager.Pop();
        internal void SetCommandLineParameters(FormattingCommandLineParameters commandLineParameters)
            Diagnostics.Assert(commandLineParameters != null, "the caller has to pass a valid instance");
            _parameters = commandLineParameters;
        /// Group transitions:
        /// none: stay in the same group
        /// enter: start a new group
        /// exit: exit from the current group.
        private enum GroupTransition { none, enter, exit, startNew }
        /// Compute the group transition, given an input object.
        /// <param name="so">Object received from the input pipeline.</param>
        /// <returns>GroupTransition enumeration.</returns>
        private GroupTransition ComputeGroupTransition(PSObject so)
            // check if we have to start a group
            if (ctx == FormattingContextState.document)
                // prime the grouping algorithm
                _viewManager.ViewGenerator.UpdateGroupingKeyValue(so);
                // need to start a group, but we are not in one
                return GroupTransition.enter;
            // check if we need to start another group and keep track
            // of the current value for the grouping property
            return _viewManager.ViewGenerator.UpdateGroupingKeyValue(so) ? GroupTransition.startNew : GroupTransition.none;
        private void WriteFormatStartData(PSObject so)
            FormatStartData startFormat = _viewManager.ViewGenerator.GenerateStartData(so);
            this.WriteObject(startFormat);
        /// Write a payplad object by properly wrapping it into
        /// a FormatEntry object.
        private void WritePayloadObject(PSObject so)
            Diagnostics.Assert(so != null, "object so cannot be null");
            FormatEntryData fed = _viewManager.ViewGenerator.GeneratePayload(so, _enumerationLimit);
            fed.writeStream = so.WriteStream;
            List<ErrorRecord> errors = _viewManager.ViewGenerator.ErrorManager.DrainFailedResultList();
        /// Inject the start group information
        /// and push group context on stack.
        /// <param name="firstObjectInGroup">current pipeline object
        /// that is starting the group</param>
        private void PushGroup(PSObject firstObjectInGroup)
            GroupStartData startGroup = _viewManager.ViewGenerator.GenerateGroupStartData(firstObjectInGroup, _enumerationLimit);
            this.WriteObject(startGroup);
            contextManager.Push(FormattingContextState.group);
        /// Inject the end group information
        /// and pop group context out of stack.
        private void PopGroup()
            GroupEndData endGroup = _viewManager.ViewGenerator.GenerateGroupEndData();
            this.WriteObject(endGroup);
        /// The formatting shape this formatter emits.
        private readonly FormatShape _shape;
        #region expression factory
        /// <exception cref="ParseException"></exception>
        internal ScriptBlock CreateScriptBlock(string scriptText)
            var scriptBlock = this.OuterCmdlet().InvokeCommand.NewScriptBlock(scriptText);
            scriptBlock.DebuggerStepThrough = true;
            return scriptBlock;
        private FormatObjectDeserializer _formatObjectDeserializer;
        private TypeInfoDataBase _typeInfoDataBase = null;
        private FormattingCommandLineParameters _parameters = null;
        private readonly FormatViewManager _viewManager = new FormatViewManager();
        private int _enumerationLimit = InitialSessionState.DefaultFormatEnumerationLimit;
    public class OuterFormatShapeCommandBase : FrontEndCommandBase
        /// Optional, non positional parameter to specify the
        /// group by property.
        public object GroupBy { get; set; } = null;
        public string View { get; set; } = null;
        public SwitchParameter ShowError
                if (showErrorsAsMessages.HasValue)
                    return showErrorsAsMessages.Value;
                showErrorsAsMessages = value;
        internal bool? showErrorsAsMessages = null;
        public SwitchParameter DisplayError
                if (showErrorsInFormattedOutput.HasValue)
                    return showErrorsInFormattedOutput.Value;
                showErrorsInFormattedOutput = value;
        internal bool? showErrorsInFormattedOutput = null;
            get { return _forceFormattingAlsoOnOutOfBand; }
            set { _forceFormattingAlsoOnOutOfBand = value; }
        private bool _forceFormattingAlsoOnOutOfBand;
        [ValidateSet(EnumerableExpansionConversion.CoreOnlyString,
                        EnumerableExpansionConversion.EnumOnlyString,
                        EnumerableExpansionConversion.BothString, IgnoreCase = true)]
        public string Expand { get; set; } = null;
        internal EnumerableExpansion? expansion = null;
        internal EnumerableExpansion? ProcessExpandParameter()
            EnumerableExpansion? retVal = null;
            if (string.IsNullOrEmpty(Expand))
            EnumerableExpansion temp;
            bool success = EnumerableExpansionConversion.Convert(Expand, out temp);
                // this should never happen, since we use the [ValidateSet] attribute
                // NOTE: this is an exception that should never be triggered
                throw PSTraceSource.NewArgumentException("Expand", FormatAndOut_MshParameter.IllegalEnumerableExpansionValue);
            retVal = temp;
            return retVal;
        internal MshParameter ProcessGroupByParameter()
            if (GroupBy != null)
                TerminatingErrorContext invocationContext =
                        new TerminatingErrorContext(this);
                ParameterProcessor processor = new ParameterProcessor(new FormatGroupByParameterDefinition());
                List<MshParameter> groupParameterList =
                    processor.ProcessParameters(new object[] { GroupBy },
                                                invocationContext);
                if (groupParameterList.Count != 0)
                    return groupParameterList[0];
            InnerFormatShapeCommand innerFormatCommand =
                            (InnerFormatShapeCommand)this.implementation;
            // read command line switches and pass them to the inner command
            FormattingCommandLineParameters parameters = GetCommandLineParameters();
            innerFormatCommand.SetCommandLineParameters(parameters);
            // must call base class for further processing
        /// It reads the command line switches and collects them into a
        /// FormattingCommandLineParameters instance, ready to pass to the
        /// inner format command.
        /// <returns>Parameters collected in unified manner.</returns>
        internal virtual FormattingCommandLineParameters GetCommandLineParameters()
        internal void ReportCannotSpecifyViewAndProperty()
            string msg = StringUtil.Format(FormatAndOut_format_xxx.CannotSpecifyViewAndPropertyError);
                "FormatCannotSpecifyViewAndProperty",
    public class OuterFormatTableAndListBase : OuterFormatShapeCommandBase
        /// Positional parameter for properties, property sets and table sets
        /// Optional parameter for excluding properties from formatting.
            FormattingCommandLineParameters parameters = new FormattingCommandLineParameters();
            GetCommandLineProperties(parameters, false);
        internal void GetCommandLineProperties(FormattingCommandLineParameters parameters, bool isTable)
                if ((Property is not null && Property.Length != 0) || (ExcludeProperty is not null && ExcludeProperty.Length != 0))
                CommandParameterDefinition def;
                if (isTable)
                    def = new FormatTableParameterDefinition();
                    def = new FormatListParameterDefinition();
                ParameterProcessor processor = new ParameterProcessor(def);
                TerminatingErrorContext invocationContext = new TerminatingErrorContext(this);
                parameters.mshParameterList = processor.ProcessParameters(Property, invocationContext);
                if (Property is null || Property.Length == 0)
                    CommandParameterDefinition def = isTable
                        ? new FormatTableParameterDefinition()
                        : new FormatListParameterDefinition();
    public class OuterFormatTableBase : OuterFormatTableAndListBase
                    return _autosize.Value;
                _autosize = value;
        /// Gets or sets if header is repeated per screen.
        public SwitchParameter RepeatHeader { get; set; }
        public SwitchParameter HideTableHeaders
                if (_hideHeaders.HasValue)
                    return _hideHeaders.Value;
                _hideHeaders = value;
        private bool? _hideHeaders = null;
        public SwitchParameter Wrap
                if (_multiLine.HasValue)
                    return _multiLine.Value;
                _multiLine = value;
        private bool? _multiLine = null;
            GetCommandLineProperties(parameters, true);
            if (RepeatHeader)
                parameters.repeatHeader = true;
            TableSpecificParameters tableParameters = new TableSpecificParameters();
            parameters.shapeParameters = tableParameters;
                tableParameters.hideHeaders = _hideHeaders.Value;
                tableParameters.multiLine = _multiLine.Value;

    class InternalHostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
        InternalHostUserInterface(PSHostUserInterface externalUI, InternalHost parentHost)
            // externalUI may be null
            _externalUI = externalUI;
            // parent may not be null, however
            Dbg.Assert(parentHost != null, "parent may not be null");
            if (parentHost == null)
                throw PSTraceSource.NewArgumentNullException(nameof(parentHost));
            _parent = parentHost;
            PSHostRawUserInterface rawui = null;
            if (externalUI != null)
                rawui = externalUI.RawUI;
            _internalRawUI = new InternalHostRawUserInterface(rawui, _parent);
            _internalRawUI.ThrowNotInteractive();
        private static void
        ThrowPromptNotInteractive(string promptMessage)
            string message = StringUtil.Format(HostInterfaceExceptionsStrings.HostFunctionPromptNotImplemented, promptMessage);
        System.Management.Automation.Host.PSHostRawUserInterface
        RawUI
                return _internalRawUI;
        public override bool SupportsVirtualTerminal
            get { return _externalUI != null && _externalUI.SupportsVirtualTerminal; }
        /// if the UI property of the external host is null, possibly because the PSHostUserInterface is not
        /// implemented by the external host.
        ReadLine()
            if (_externalUI == null)
                result = _externalUI.ReadLine();
                LocalPipeline lpl = (LocalPipeline)((RunspaceBase)_parent.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
        SecureString
        ReadLineAsSecureString()
            SecureString result = null;
                result = _externalUI.ReadLineAsSecureString();
        /// if <paramref name="value"/> is not null and the UI property of the external host is null,
        ///     possibly because the PSHostUserInterface is not implemented by the external host
            _externalUI.Write(value);
        /// <param name="foregroundColor">
        /// <param name="backgroundColor">
        Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            if (PSStyle.Instance.OutputRendering == OutputRendering.PlainText)
                _externalUI.Write(foregroundColor, backgroundColor, value);
        /// <seealso cref="Write(string)"/>
        /// <seealso cref="WriteLine(string)"/>
        ///     implemented by the external host
        WriteLine()
            _externalUI.WriteLine();
            _externalUI.WriteLine(value);
        WriteErrorLine(string value)
            _externalUI.WriteErrorLine(value);
        WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
                _externalUI.WriteLine(foregroundColor, backgroundColor, value);
        /// if <paramref name="message"/> is not null and the UI property of the external host is null,
        WriteDebugLine(string message)
            WriteDebugLineHelper(message);
        internal void WriteDebugRecord(DebugRecord record)
            WriteDebugInfoBuffers(record);
            _externalUI.WriteDebugLine(record.Message);
        /// Writes the DebugRecord to informational buffers.
        /// <param name="record">DebugRecord.</param>
        internal void WriteDebugInfoBuffers(DebugRecord record) => _informationalBuffers?.AddDebug(record);
        /// Helper function for WriteDebugLine.
        /// If the debug preference is set to ActionPreference.Stop
        /// If the debug preference is set to ActionPreference.Inquire and user requests to stop execution.
        /// If the debug preference is not a valid ActionPreference value.
        WriteDebugLine(string message, ref ActionPreference preference)
                    if (!DebugShouldContinue(message, ref preference))
                        // user asked to exit with an error
                        errorMsg = InternalHostUserInterfaceStrings.WriteDebugLineStoppedError;
                        errorRecord = new ErrorRecord(new ParentContainsErrorRecordException(errorMsg),
                            "UserStopRequest", ErrorCategory.OperationStopped, null);
                        ActionPreferenceStopException e = new ActionPreferenceStopException(errorRecord);
                        // We cannot call ThrowTerminatingError since this is not a cmdlet or provider
                        "ActionPreferenceStop", ErrorCategory.OperationStopped, null);
                    ActionPreferenceStopException ense = new ActionPreferenceStopException(errorRecord);
                    throw ense;
                    Dbg.Assert(false, "all preferences should be checked");
                    throw PSTraceSource.NewArgumentException(nameof(preference),
                        InternalHostUserInterfaceStrings.UnsupportedPreferenceError, preference);
                    // break;
        /// If informationBuffers is not null, the respective messages will also
        /// be written to the buffers along with external host.
        /// <param name="informationalBuffers">
        /// Buffers to which Debug, Verbose, Warning, Progress, Information messages
        /// will be written to.
        /// This method is not thread safe. Caller should make sure of the
        /// associated risks.
        internal void SetInformationalMessageBuffers(PSInformationalBuffers informationalBuffers)
            _informationalBuffers = informationalBuffers;
        /// Gets the informational message buffers of the host.
        /// <returns>Informational message buffers.</returns>
        internal PSInformationalBuffers GetInformationalMessageBuffers()
            return _informationalBuffers;
        WriteDebugLineHelper(string message)
            if (message == null)
            WriteDebugRecord(new DebugRecord(message));
        /// Ask the user whether to continue/stop or break to a nested prompt.
        /// Message to display to the user. This routine will append the text "Continue" to ensure that people know what question
        /// they are answering.
        /// <param name="actionPreference">
        /// Preference setting which determines the behaviour.  This is by-ref and will be modified based upon what the user
        /// types. (e.g. YesToAll will change Inquire => NotifyContinue)
        DebugShouldContinue(string message, ref ActionPreference actionPreference)
            Dbg.Assert(actionPreference == ActionPreference.Inquire, "Why are you inquiring if your preference is not to?");
            bool shouldContinue = false;
            Collection<ChoiceDescription> choices = new Collection<ChoiceDescription>();
            choices.Add(new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueYesLabel, InternalHostUserInterfaceStrings.ShouldContinueYesHelp));
            choices.Add(new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueYesToAllLabel, InternalHostUserInterfaceStrings.ShouldContinueYesToAllHelp));
            choices.Add(new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueNoLabel, InternalHostUserInterfaceStrings.ShouldContinueNoHelp));
            choices.Add(new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueNoToAllLabel, InternalHostUserInterfaceStrings.ShouldContinueNoToAllHelp));
            choices.Add(new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueSuspendLabel, InternalHostUserInterfaceStrings.ShouldContinueSuspendHelp));
            bool endLoop = true;
                endLoop = true;
                switch (
                    PromptForChoice(
                        InternalHostUserInterfaceStrings.ShouldContinuePromptMessage,
                        choices,
                        0))
                        actionPreference = ActionPreference.Continue;
                        // No to All means that we want to stop every time WriteDebug is called. Since No throws an error, I
                        // think that ordinarily, the caller will terminate.  So I don't think the caller will ever get back
                        // calling WriteDebug again, and thus "No to All" might not be a useful option to have.
                        actionPreference = ActionPreference.Stop;
                        _parent.EnterNestedPrompt();
                        endLoop = false;
            } while (!endLoop);
            return shouldContinue;
        /// if <paramref name="record"/> is not null and the UI property of the external host is null,
        WriteProgress(Int64 sourceId, ProgressRecord record)
            if (record == null)
                throw PSTraceSource.NewArgumentNullException(nameof(record));
            // Write to Information Buffers
            _informationalBuffers?.AddProgress(record);
            _externalUI.WriteProgress(sourceId, record);
        WriteVerboseLine(string message)
            WriteVerboseRecord(new VerboseRecord(message));
        internal void WriteVerboseRecord(VerboseRecord record)
            WriteVerboseInfoBuffers(record);
            _externalUI.WriteVerboseLine(record.Message);
        /// Writes the VerboseRecord to informational buffers.
        /// <param name="record">VerboseRecord.</param>
        internal void WriteVerboseInfoBuffers(VerboseRecord record) => _informationalBuffers?.AddVerbose(record);
            WriteWarningRecord(new WarningRecord(message));
        internal void WriteWarningRecord(WarningRecord record)
            WriteWarningInfoBuffers(record);
            _externalUI.WriteWarningLine(record.Message);
        /// Writes the WarningRecord to informational buffers.
        /// <param name="record">WarningRecord.</param>
        internal void WriteWarningInfoBuffers(WarningRecord record) => _informationalBuffers?.AddWarning(record);
        internal void WriteInformationRecord(InformationRecord record)
            WriteInformationInfoBuffers(record);
            _externalUI.WriteInformation(record);
        /// Writes the InformationRecord to informational buffers.
        internal void WriteInformationInfoBuffers(InformationRecord record) => _informationalBuffers?.AddInformation(record);
        internal static Type GetFieldType(FieldDescription field)
            if (TypeResolver.TryResolveType(field.ParameterAssemblyFullName, out result) ||
                TypeResolver.TryResolveType(field.ParameterTypeFullName, out result))
        internal static bool IsSecuritySensitiveType(string typeName)
            if (typeName.Equals(nameof(PSCredential), StringComparison.OrdinalIgnoreCase))
            if (typeName.Equals(nameof(SecureString), StringComparison.OrdinalIgnoreCase))
        /// <param name="descriptions">
        /// If <paramref name="descriptions"/> is null.
        /// If <paramref name="descriptions"/>.Count is less than 1.
        /// if the UI property of the external host is null,
                throw PSTraceSource.NewArgumentException(nameof(descriptions), InternalHostUserInterfaceStrings.PromptEmptyDescriptionsError, "descriptions");
                ThrowPromptNotInteractive(message);
            Dictionary<string, PSObject> result = null;
                result = _externalUI.Prompt(caption, message, descriptions);
        /// <param name="defaultChoice">
        PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
                result = _externalUI.PromptForChoice(caption, message, choices, defaultChoice);
            IHostUISupportsMultipleChoiceSelection hostForMultipleChoices =
                _externalUI as IHostUISupportsMultipleChoiceSelection;
            Collection<int> result = null;
                if (hostForMultipleChoices == null)
                    // host did not implement this new interface..
                    // so work with V1 host API to get the behavior..
                    // this will allow Hosts that were developed with
                    // V1 API to interact with PowerShell V2.
                    result = EmulatePromptForMultipleChoice(caption, message, choices, defaultChoices);
                    result = hostForMultipleChoices.PromptForChoice(caption, message, choices, defaultChoices);
        /// This method is added to be backward compatible with V1 hosts w.r.t
        /// new PromptForChoice method added in PowerShell V2.
        /// <param name="defaultChoices"></param>
        /// 1. Choices is null.
        /// 2. Choices.Count = 0
        /// 3. DefaultChoice is either less than 0 or greater than Choices.Count
        private Collection<int> EmulatePromptForMultipleChoice(string caption,
            Dbg.Assert(_externalUI != null, "externalUI cannot be null.");
                    InternalHostUserInterfaceStrings.EmptyChoicesError, "choices");
                            InternalHostUserInterfaceStrings.InvalidDefaultChoiceForMultipleSelection,
            // Construct the caption + message + list of choices + default choices
            Text.StringBuilder choicesMessage = new Text.StringBuilder();
            const char newLine = '\n';
                choicesMessage.Append(caption);
                choicesMessage.Append(newLine);
                choicesMessage.Append(message);
                        Globalization.CultureInfo.InvariantCulture,
                choicesMessage.Append(choice);
            // default choices
                Text.StringBuilder defaultChoicesBuilder = new Text.StringBuilder();
                    defaultChoicesBuilder.Append(Globalization.CultureInfo.InvariantCulture, $"{prepend}{defaultStr}");
                string defaultChoicesStr = defaultChoicesBuilder.ToString();
                    defaultPrompt = StringUtil.Format(InternalHostUserInterfaceStrings.DefaultChoice, defaultChoicesStr);
                    defaultPrompt = StringUtil.Format(InternalHostUserInterfaceStrings.DefaultChoicesForMultipleChoices,
                        defaultChoicesStr);
            string messageToBeDisplayed = choicesMessage.ToString() + defaultPrompt + newLine;
            // read choices from the user
                string choiceMsg = StringUtil.Format(InternalHostUserInterfaceStrings.ChoiceMessage, choicesSelected);
                messageToBeDisplayed += choiceMsg;
                _externalUI.WriteLine(messageToBeDisplayed);
                string response = _externalUI.ReadLine();
                // reset messageToBeDisplayed
                messageToBeDisplayed = string.Empty;
        private readonly PSHostUserInterface _externalUI = null;
        private readonly InternalHostRawUserInterface _internalRawUI = null;
        private readonly InternalHost _parent = null;
        private PSInformationalBuffers _informationalBuffers = null;

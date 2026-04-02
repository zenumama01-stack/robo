using InternalHostUserInterface = System.Management.Automation.Internal.Host.InternalHostUserInterface;
    class ConsoleHostUserInterface : System.Management.Automation.Host.PSHostUserInterface
        /// Used by Prompt to indicate any common errors when converting the user input string to
        ///  the type of the parameter.
        private enum PromptCommonInputErrors
            /// No error or not an error prompt handles.
            /// Format error.
            Format,
            /// Overflow error.
            Overflow
        AtLeastOneHelpMessageIsPresent(Collection<FieldDescription> descriptions)
            foreach (FieldDescription fd in descriptions)
                if (fd != null)
                    if (!string.IsNullOrEmpty(fd.HelpMessage))
        /// If <paramref name="descriptions"/> is null
        ///    at least one FieldDescription in <paramref name="descriptions"/> is null
        /// If <paramref name="descriptions"/> count is less than 1
        ///    at least one FieldDescription.AssemblyFullName in <paramref name="descriptions"/> is
        ///     null or empty
        /// <exception cref="PromptingException">
        /// If a FieldDescription in <paramref name="descriptions"/> specifies one of SecureString or
        ///     PSCredential and the type can not be loaded.
        ///    at least one FieldDescription in <paramref name="descriptions"/> specifies an array
        ///     whose rank is less than 1.
        /// <exception cref="PSInvalidCastException">
        /// If the converting the user input to the prompt field type fails unless it is caused by
        ///     OverflowException or FormatException
        Dictionary<string, PSObject>
        Prompt(string caption, string message, Collection<FieldDescription> descriptions)
            // Need to implement EchoOnPrompt
            if (descriptions == null)
                throw PSTraceSource.NewArgumentNullException(nameof(descriptions));
            if (descriptions.Count < 1)
                throw PSTraceSource.NewArgumentException(nameof(descriptions),
                    ConsoleHostUserInterfaceStrings.PromptEmptyDescriptionsErrorTemplate, "descriptions");
                Dictionary<string, PSObject> results = new Dictionary<string, PSObject>();
                bool cancelInput = false;
                if (!string.IsNullOrEmpty(caption))
                    // Should be a skin lookup
                    WriteLineToConsole(PromptColor, RawUI.BackgroundColor, WrapToCurrentWindowWidth(caption));
                    WriteLineToConsole(WrapToCurrentWindowWidth(message));
                if (AtLeastOneHelpMessageIsPresent(descriptions))
                    WriteLineToConsole(WrapToCurrentWindowWidth(ConsoleHostUserInterfaceStrings.PromptHelp));
                int descIndex = -1;
                foreach (FieldDescription desc in descriptions)
                    descIndex++;
                    if (desc == null)
                            ConsoleHostUserInterfaceStrings.NullErrorTemplate,
                            string.Create(CultureInfo.InvariantCulture, $"descriptions[{descIndex}]"));
                    PSObject inputPSObject = null;
                    string fieldPrompt = null;
                    fieldPrompt = desc.Name;
                    // FieldDescription.ParameterAssemblyFullName never returns null. But this is
                    // defense in depth.
                    if (string.IsNullOrEmpty(desc.ParameterAssemblyFullName))
                        string paramName =
                            string.Create(CultureInfo.InvariantCulture, $"descriptions[{descIndex}].AssemblyFullName");
                        throw PSTraceSource.NewArgumentException(paramName, ConsoleHostUserInterfaceStrings.NullOrEmptyErrorTemplate, paramName);
                    Type fieldType = InternalHostUserInterface.GetFieldType(desc);
                    if (fieldType == null)
                        if (InternalHostUserInterface.IsSecuritySensitiveType(desc.ParameterTypeName))
                            string errMsg =
                                StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptTypeLoadErrorTemplate,
                                    desc.Name, desc.ParameterTypeFullName);
                            PromptingException e = new PromptingException(errMsg,
                                null, "BadTypeName", ErrorCategory.InvalidType);
                        fieldType = typeof(string);
                    if (fieldType.GetInterface(typeof(IList).FullName) != null)
                        // field is a type implementing IList
                        ArrayList inputList = new ArrayList(); // stores all converted user input before
                        // assigned to an array
                        // if the field is an array, the element type can be found; else, use Object
                        Type elementType = typeof(object);
                        if (fieldType.IsArray)
                            elementType = fieldType.GetElementType();
                            int rank = fieldType.GetArrayRank();
                            // This check may be redundant because it doesn't seem possible to create
                            // an array of zero dimension.
                            if (rank <= 0)
                                string msg = StringUtil.Format(ConsoleHostUserInterfaceStrings.RankZeroArrayErrorTemplate, desc.Name);
                                ArgumentException innerException = PSTraceSource.NewArgumentException(
                                    string.Create(CultureInfo.InvariantCulture, $"descriptions[{descIndex}].AssemblyFullName"));
                                PromptingException e = new PromptingException(msg, innerException, "ZeroRankArray", ErrorCategory.InvalidOperation);
                        StringBuilder fieldPromptList = new StringBuilder(fieldPrompt);
                        // fieldPromptList = fieldPrompt + "[i] :"
                        fieldPromptList.Append('[');
                            fieldPromptList.Append(CultureInfo.InvariantCulture, $"{inputList.Count}]: ");
                            bool endListInput = false;
                            object convertedObj = null;
                            _ = PromptForSingleItem(
                                elementType,
                                fieldPromptList.ToString(),
                                fieldPrompt,
                                message,
                                desc,
                                fieldEchoOnPrompt: true,
                                listInput: true,
                                out endListInput,
                                out cancelInput,
                                out convertedObj);
                            if (cancelInput || endListInput)
                            else if (!cancelInput)
                                inputList.Add(convertedObj);
                                // Remove the indices from the prompt
                                fieldPromptList.Length = fieldPrompt.Length + 1;
                        // if cancelInput, should throw OperationCancelException?
                        if (!cancelInput)
                            object tryConvertResult = null;
                            if (LanguagePrimitives.TryConvertTo(inputList, fieldType, out tryConvertResult))
                                inputPSObject = PSObject.AsPSObject(tryConvertResult);
                                inputPSObject = PSObject.AsPSObject(inputList);
                        string printFieldPrompt = StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptFieldPromptInputSeparatorTemplate,
                            fieldPrompt);
                        // field is not a list
                            fieldType,
                            printFieldPrompt,
                            listInput: false,
                            endListInput: out _,
                            inputPSObject = PSObject.AsPSObject(convertedObj);
                    if (cancelInput)
                        s_tracer.WriteLine("Prompt canceled");
                        results.Clear();
                    results.Add(desc.Name, PSObject.AsPSObject(inputPSObject));
        private string PromptForSingleItem(Type fieldType,
            string printFieldPrompt,
            string fieldPrompt,
            string caption,
            FieldDescription desc,
            bool fieldEchoOnPrompt,
            bool listInput,
            out bool endListInput,
            out bool cancelInput,
            out object convertedObj
            cancelInput = false;
            endListInput = false;
            convertedObj = null;
            if (fieldType.Equals(typeof(SecureString)))
                WriteToConsole(printFieldPrompt, true);
                SecureString secureString = ReadLineAsSecureString();
                convertedObj = secureString;
                cancelInput = (convertedObj == null);
                if ((secureString != null) && (secureString.Length == 0) && listInput)
                    endListInput = true;
            else if (fieldType.Equals(typeof(PSCredential)))
                WriteLineToConsole(WrapToCurrentWindowWidth(fieldPrompt));
                PSCredential credential = null;
                credential =
                    PromptForCredential(
                        null,   // caption already written
                        null,   // message already written
                convertedObj = credential;
                if ((credential != null) && (credential.Password.Length == 0) && listInput)
                string inputString = null;
                    inputString = PromptReadInput(
                        printFieldPrompt, desc, fieldEchoOnPrompt,
                        listInput, out endListInput, out cancelInput);
                while (!cancelInput && !endListInput && PromptTryConvertTo(fieldType, desc.IsFromRemoteHost, inputString, out convertedObj) !=
                    PromptCommonInputErrors.None);
                return inputString;
        /// Called by Prompt. Reads user input and processes tilde commands.
        /// <param name="fieldPrompt">Prompt written to host for the field.</param>
        /// <param name="desc">The field to be read.</param>
        /// <param name="fieldEchoOnPrompt">True to echo user input.</param>
        /// <param name="listInput">True if the field is a list.</param>
        /// <param name="endListInput">Valid only if listInput is true. set to true if the input signals end of list input.</param>
        /// <param name="cancelled">True if-and-only-if the input is canceled, e.g., by Ctrl-C or Ctrl-Break.</param>
        /// <returns>Processed input string to be converted with LanguagePrimitives.ConvertTo.</returns>
        private string PromptReadInput(string fieldPrompt, FieldDescription desc, bool fieldEchoOnPrompt,
                        bool listInput, out bool endListInput, out bool cancelled)
            Dbg.Assert(fieldPrompt != null, "fieldPrompt should never be null when PromptReadInput is called");
            Dbg.Assert(desc != null, "desc should never be null when PromptReadInput is called");
            string processedInputString = null;
            cancelled = false;
            bool inputDone = false;
            while (!inputDone)
                WriteToConsole(fieldPrompt, true);
                string rawInputString = null;
                // Implement no echo here.
                if (fieldEchoOnPrompt)
                    rawInputString = ReadLine();
                    object userInput = ReadLineSafe(false, null);
                    string userInputString = userInput as string;
                    System.Management.Automation.Diagnostics.Assert(userInputString != null, "ReadLineSafe did not return a string");
                    rawInputString = userInputString;
                if (rawInputString == null)
                    // processedInputString is null as well. No need to assign null to it.
                    cancelled = true;
                if (!string.IsNullOrEmpty(desc.Label) && rawInputString.StartsWith(PromptCommandPrefix, StringComparison.Ordinal))
                    processedInputString = PromptCommandMode(rawInputString, desc, out inputDone);
                    if (rawInputString.Length == 0 && listInput)
                    processedInputString = rawInputString;
            return processedInputString;
        /// Uses LanguagePrimitives.ConvertTo to parse inputString for fieldType. Handles two most common parse
        ///  exceptions: OverflowException and FormatException.
        /// <param name="fieldType">The type that inputString is to be interpreted.</param>
        /// <param name="isFromRemoteHost">Is the call coming from a remote host.</param>
        /// <param name="inputString">The string to be converted.</param>
        /// <param name="convertedObj">if there's no error in the conversion, the converted object will be assigned here;
        /// otherwise, this will be the same as the inputString</param>
        /// <returns>An object of type fieldType that inputString represents.</returns>
        private PromptCommonInputErrors PromptTryConvertTo(Type fieldType, bool isFromRemoteHost, string inputString, out object convertedObj)
            Dbg.Assert(fieldType != null, "fieldType should never be null when PromptTryConvertTo is called");
            convertedObj = inputString;
            // do not do any type conversion if the prompt request is coming from a remote host
            // (bug Windows 7: #381643) + its bad to have potential side effects from casting on the client (think casting to a FileStream)
            if (isFromRemoteHost)
                return PromptCommonInputErrors.None;
                convertedObj = LanguagePrimitives.ConvertTo(inputString, fieldType, CultureInfo.InvariantCulture);
                Exception innerE = e.InnerException;
                if (innerE != null)
                    if (innerE is OverflowException)
                        string errMsgTemplate =
                            ConsoleHostUserInterfaceStrings.PromptParseOverflowErrorTemplate;
                        WriteLineToConsole(
                            WrapToCurrentWindowWidth(
                                string.Format(CultureInfo.CurrentCulture, errMsgTemplate, fieldType, inputString)));
                        return PromptCommonInputErrors.Overflow;
                    else if (innerE is FormatException)
                        // Don't output error message if the inputString is empty
                        if (inputString.Length > 0)
                                ConsoleHostUserInterfaceStrings.PromptParseFormatErrorTemplate;
                        return PromptCommonInputErrors.Format;
        /// Handles Tilde Commands in Prompt
        /// If input does not start with PromptCommandPrefix (= "!"), returns input
        /// Tilde commands -
        /// !   end of list, only valid for input field types that implement IList, returns string.Empty
        /// !!* input !* literally, returns !* where * is any string
        /// !h  prints out field's Quick Help, returns null
        /// All others tilde comments are invalid and return null
        /// returns null if-and-only-if there's nothing the caller can process.
        /// <param name="desc"></param>
        /// <param name="inputDone"></param>
        private string PromptCommandMode(string input, FieldDescription desc, out bool inputDone)
            Dbg.Assert(input != null && input.StartsWith(PromptCommandPrefix, StringComparison.OrdinalIgnoreCase),
                string.Create(CultureInfo.InvariantCulture, $"input should start with {PromptCommandPrefix}"));
            Dbg.Assert(desc != null, "desc should never be null when PromptCommandMode is called");
            string command = input.Substring(1);
            inputDone = true;
            if (command.StartsWith(PromptCommandPrefix, StringComparison.OrdinalIgnoreCase))
                return command;
            if (command.Length == 1)
                if (command[0] == '?')
                    if (string.IsNullOrEmpty(desc.HelpMessage))
                        string noHelpErrMsg =
                            StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptNoHelpAvailableErrorTemplate, desc.Name);
                        s_tracer.TraceWarning(noHelpErrMsg);
                        WriteLineToConsole(WrapToCurrentWindowWidth(noHelpErrMsg));
                        WriteLineToConsole(WrapToCurrentWindowWidth(desc.HelpMessage));
                    ReportUnrecognizedPromptCommand(input);
                inputDone = false;
            if (command.Length == 2)
                if (string.Equals(command, "\"\"", StringComparison.OrdinalIgnoreCase))
            if (string.Equals(command, "$null", StringComparison.OrdinalIgnoreCase))
        private void ReportUnrecognizedPromptCommand(string command)
            string msg = StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptUnrecognizedCommandErrorTemplate, command);
            WriteLineToConsole(WrapToCurrentWindowWidth(msg));
        // Prefix for command mode in Prompt
        private const string PromptCommandPrefix = "!";

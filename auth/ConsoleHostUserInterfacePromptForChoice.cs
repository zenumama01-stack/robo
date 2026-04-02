    internal partial class ConsoleHostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
        /// If <paramref name="choices"/> is null.
        /// If <paramref name="choices"/>.Count is 0.
        /// If <paramref name="defaultChoice"/> is greater than
        ///     the length of <paramref name="choices"/>.
        ///  when prompt is canceled by, for example, Ctrl-c.
            if (choices == null)
                throw PSTraceSource.NewArgumentNullException(nameof(choices));
            if (choices.Count == 0)
                throw PSTraceSource.NewArgumentException(nameof(choices),
                    ConsoleHostUserInterfaceStrings.EmptyChoicesErrorTemplate, "choices");
            if ((defaultChoice < -1) || (defaultChoice >= choices.Count))
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(defaultChoice), defaultChoice,
                    ConsoleHostUserInterfaceStrings.InvalidDefaultChoiceErrorTemplate, "defaultChoice", "choice");
                int result = defaultChoice;
                string[,] hotkeysAndPlainLabels = null;
                HostUIHelperMethods.BuildHotkeysAndPlainLabels(choices, out hotkeysAndPlainLabels);
                Dictionary<int, bool> defaultChoiceKeys = new Dictionary<int, bool>();
                // add the default choice key only if it is valid. -1 is used to specify
                // no default.
                if (defaultChoice >= 0)
                    defaultChoiceKeys.Add(defaultChoice, true);
                    WriteChoicePrompt(hotkeysAndPlainLabels, defaultChoiceKeys, false);
                    ReadLineResult rlResult;
                    string response = ReadChoiceResponse(out rlResult);
                    if (rlResult == ReadLineResult.endedOnBreak)
                        string msg = ConsoleHostUserInterfaceStrings.PromptCanceledError;
                        PromptingException e = new PromptingException(
                            msg, null, "PromptForChoiceCanceled", ErrorCategory.OperationStopped);
                    if (response.Length == 0)
                        // they just hit enter.
                            // if there's a default, pick that one.
                            result = defaultChoice;
                    // decide which choice they made.
                    if (response.Trim() == "?")
                        // show the help
                        ShowChoiceHelp(choices, hotkeysAndPlainLabels);
                    result = HostUIHelperMethods.DetermineChoicePicked(response.Trim(), choices, hotkeysAndPlainLabels);
                    if (result >= 0)
                    // their input matched none of the choices, so prompt again
        /// Presents a dialog allowing the user to choose options from a set of options.
        /// <param name="caption">
        /// Caption to precede or title the prompt.  E.g. "Parameters for get-foo (instance 1 of 2)"
        /// A message that describes what the choice is for.
        /// <param name="choices">
        /// An Collection of ChoiceDescription objects that describe each choice.
        /// <param name="defaultChoices">
        /// The index of the labels in the choices collection element to be presented to the user as
        /// the default choice(s).
        /// The indices of the choice elements that corresponds to the options selected.
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.PromptForChoice"/>
        public Collection<int> PromptForChoice(string caption,
            Collection<ChoiceDescription> choices,
            IEnumerable<int> defaultChoices)
            if (defaultChoices != null)
                foreach (int defaultChoice in defaultChoices)
                    if ((defaultChoice < 0) || (defaultChoice >= choices.Count))
                        throw PSTraceSource.NewArgumentOutOfRangeException("defaultChoice", defaultChoice,
                            ConsoleHostUserInterfaceStrings.InvalidDefaultChoiceForMultipleSelection,
                            "defaultChoice",
                            "choices",
                            defaultChoice);
                    defaultChoiceKeys.TryAdd(defaultChoice, true);
            Collection<int> result = new Collection<int>();
                // write caption on the console, if present.
                // write message
                WriteChoicePrompt(hotkeysAndPlainLabels, defaultChoiceKeys, true);
                if (defaultChoiceKeys.Count > 0)
                // used to display ChoiceMessage like Choice[0],Choice[1] etc
                int choicesSelected = 0;
                    // write the current prompt
                    string choiceMsg = StringUtil.Format(ConsoleHostUserInterfaceStrings.ChoiceMessage, choicesSelected);
                    WriteToConsole(PromptColor, RawUI.BackgroundColor, WrapToCurrentWindowWidth(choiceMsg));
                    // they just hit enter
                        // this may happen when
                        // 1. user wants to go with the defaults
                        // 2. user selected some choices and wanted those
                        // choices to be picked.
                        // user did not pick up any choices..choose the default
                        if (result.Count == 0)
                            foreach (int defaultChoice in defaultChoiceKeys.Keys)
                                result.Add(defaultChoice);
                        // allow for no choice selection.
                    int choicePicked = HostUIHelperMethods.DetermineChoicePicked(response.Trim(), choices, hotkeysAndPlainLabels);
                    if (choicePicked >= 0)
                        result.Add(choicePicked);
                        choicesSelected++;
                    // prompt for multiple choices
        private void WriteChoicePrompt(string[,] hotkeysAndPlainLabels,
            Dictionary<int, bool> defaultChoiceKeys,
            bool shouldEmulateForMultipleChoiceSelection)
            System.Management.Automation.Diagnostics.Assert(defaultChoiceKeys != null, "defaultChoiceKeys cannot be null.");
            int lineLenMax = RawUI.WindowSize.Width - 1;
            int lineLen = 0;
            const string choiceTemplate = "[{0}] {1}  ";
            for (int i = 0; i < hotkeysAndPlainLabels.GetLength(1); ++i)
                ConsoleColor cfg = PromptColor;
                if (defaultChoiceKeys.ContainsKey(i))
                    cfg = DefaultPromptColor;
                string choice =
                        choiceTemplate,
                        hotkeysAndPlainLabels[0, i],
                        hotkeysAndPlainLabels[1, i]);
                WriteChoiceHelper(choice, cfg, bg, ref lineLen, lineLenMax);
                if (shouldEmulateForMultipleChoiceSelection)
            WriteChoiceHelper(
                ConsoleHostUserInterfaceStrings.PromptForChoiceHelp,
                fg,
                bg,
                ref lineLen,
                lineLenMax);
            string defaultPrompt = string.Empty;
                string prepend = string.Empty;
                StringBuilder defaultChoicesBuilder = new StringBuilder();
                    string defaultStr = hotkeysAndPlainLabels[0, defaultChoice];
                    if (string.IsNullOrEmpty(defaultStr))
                        defaultStr = hotkeysAndPlainLabels[1, defaultChoice];
                    defaultChoicesBuilder.Append(CultureInfo.InvariantCulture, $"{prepend}{defaultStr}");
                    prepend = ",";
                string defaultChoices = defaultChoicesBuilder.ToString();
                if (defaultChoiceKeys.Count == 1)
                    defaultPrompt = shouldEmulateForMultipleChoiceSelection ?
                        StringUtil.Format(ConsoleHostUserInterfaceStrings.DefaultChoiceForMultipleChoices, defaultChoices)
                        StringUtil.Format(ConsoleHostUserInterfaceStrings.DefaultChoicePrompt, defaultChoices);
                    defaultPrompt = StringUtil.Format(ConsoleHostUserInterfaceStrings.DefaultChoicesForMultipleChoices,
                        defaultChoices);
            WriteChoiceHelper(defaultPrompt,
        private void WriteChoiceHelper(string text, ConsoleColor fg, ConsoleColor bg, ref int lineLen, int lineLenMax)
            int textLen = RawUI.LengthInBufferCells(text);
            bool trimEnd = false;
            if (lineLen + textLen > lineLenMax)
                trimEnd = true;
                lineLen = textLen;
                lineLen += textLen;
            WriteToConsole(fg, bg, trimEnd ? text.TrimEnd(null) : text);
        private string ReadChoiceResponse(out ReadLineResult result)
            return InternalTestHooks.ForcePromptForChoiceDefaultOption
                   ? string.Empty
                   : ReadLine(
                       endOnTab: false,
                       initialContent: string.Empty,
                       result: out result,
                       calledFromPipeline: true,
                       transcribeResult: true);
        private void ShowChoiceHelp(Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
            Dbg.Assert(choices != null, "choices: expected a value");
            Dbg.Assert(hotkeysAndPlainLabels != null, "hotkeysAndPlainLabels: expected a value");
            for (int i = 0; i < choices.Count; ++i)
                string s;
                // If there's no hotkey, use the label as the help
                if (hotkeysAndPlainLabels[0, i].Length > 0)
                    s = hotkeysAndPlainLabels[0, i];
                    s = hotkeysAndPlainLabels[1, i];
                        string.Create(CultureInfo.InvariantCulture, $"{s} - {choices[i].HelpMessage}")));
        /// Guarantee a contrasting color for the prompt...
        private ConsoleColor PromptColor
                switch (RawUI.BackgroundColor)
                    case ConsoleColor.White: return ConsoleColor.Black;
                    case ConsoleColor.Cyan: return ConsoleColor.Black;
                    case ConsoleColor.DarkYellow: return ConsoleColor.Black;
                    case ConsoleColor.Yellow: return ConsoleColor.Black;
                    case ConsoleColor.Gray: return ConsoleColor.Black;
                    case ConsoleColor.Green: return ConsoleColor.Black;
                    default: return ConsoleColor.White;
        /// Guarantee a contrasting color for the default prompt that is slightly
        /// different from the other prompt elements.
        private ConsoleColor DefaultPromptColor
                if (PromptColor == ConsoleColor.White)
                    return ConsoleColor.Yellow;
                    return ConsoleColor.Blue;

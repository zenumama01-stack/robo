    /// Provides a set of possible completions for given input.
    public class CommandCompletion
        /// Construct the result CompleteInput or TabExpansion2.
        public CommandCompletion(Collection<CompletionResult> matches, int currentMatchIndex, int replacementIndex, int replacementLength)
            this.CompletionMatches = matches;
            this.CurrentMatchIndex = currentMatchIndex;
            this.ReplacementIndex = replacementIndex;
            this.ReplacementLength = replacementLength;
        #region Fields and Properties
        /// Current index in <see cref="CompletionMatches"/>.
        public int CurrentMatchIndex { get; set; }
        /// Returns the starting replacement index from the original input.
        public int ReplacementIndex { get; set; }
        /// Returns the length of the text to replace from the original input.
        public int ReplacementLength { get; set; }
        /// Gets all the completion results.
        public Collection<CompletionResult> CompletionMatches { get; set; }
        internal static readonly IList<CompletionResult> EmptyCompletionResult = Array.Empty<CompletionResult>();
        private static readonly CommandCompletion s_emptyCommandCompletion = new CommandCompletion(
            new Collection<CompletionResult>(EmptyCompletionResult), -1, -1, -1);
        #endregion Fields and Properties
        /// <param name="cursorIndex"></param>
        public static Tuple<Ast, Token[], IScriptPosition> MapStringInputToParsedInput(string input, int cursorIndex)
            if (cursorIndex > input.Length)
                throw PSTraceSource.NewArgumentException(nameof(cursorIndex));
            var ast = Parser.ParseInput(input, out tokens, out errors);
            IScriptPosition cursorPosition =
                ((InternalScriptPosition)ast.Extent.StartScriptPosition).CloneWithNewOffset(cursorIndex);
            return Tuple.Create<Ast, Token[], IScriptPosition>(ast, tokens, cursorPosition);
        /// <param name="input">The input to complete.</param>
        /// <param name="cursorIndex">The index of the cursor in the input.</param>
        /// <param name="options">Optional options to configure how completion is performed.</param>
        public static CommandCompletion CompleteInput(string input, int cursorIndex, Hashtable options)
            if (input == null || input.Length == 0)
                return s_emptyCommandCompletion;
            var parsedInput = MapStringInputToParsedInput(input, cursorIndex);
            return CompleteInputImpl(parsedInput.Item1, parsedInput.Item2, parsedInput.Item3, options);
        /// <param name="ast">Ast for pre-parsed input.</param>
        /// <param name="tokens">Tokens for pre-parsed input.</param>
        /// <param name="positionOfCursor"></param>
        public static CommandCompletion CompleteInput(Ast ast, Token[] tokens, IScriptPosition positionOfCursor, Hashtable options)
            if (ast == null)
                throw PSTraceSource.NewArgumentNullException(nameof(ast));
            if (tokens == null)
                throw PSTraceSource.NewArgumentNullException(nameof(tokens));
            if (positionOfCursor == null)
                throw PSTraceSource.NewArgumentNullException(nameof(positionOfCursor));
            if (ast.Extent.Text.Length == 0)
            return CompleteInputImpl(ast, tokens, positionOfCursor, options);
        /// Invokes the script function TabExpansion2.
        /// <param name="input">The input script to complete.</param>
        /// <param name="cursorIndex">The offset in <paramref name="input"/> where completion is requested.</param>
        /// <param name="options">Optional parameter that specifies configurable options for completion.</param>
        /// <param name="powershell">The powershell to use to invoke the script function TabExpansion2.</param>
        /// <returns>A collection of completions with the replacement start and length.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "powershell")]
        public static CommandCompletion CompleteInput(string input, int cursorIndex, Hashtable options, PowerShell powershell)
            if (powershell == null)
                throw PSTraceSource.NewArgumentNullException(nameof(powershell));
            // If we are in a debugger stop, let the debugger do the command completion.
            var debugger = powershell.Runspace?.Debugger;
                return CompleteInputInDebugger(input, cursorIndex, options, debugger);
            var remoteRunspace = powershell.Runspace as RemoteRunspace;
            if (remoteRunspace != null)
                // If the runspace is not available to run commands then exit here because nested commands are not
                // supported on remote runspaces.
                if (powershell.IsNested || (remoteRunspace.RunspaceAvailability != RunspaceAvailability.Available))
                // If it's in the nested prompt, the powershell instance is created by "PowerShell.Create(RunspaceMode.CurrentRunspace);".
                // In this case, the powershell._runspace is null but if we try to access the property "Runspace", it will create a new
                // local runspace - the default local runspace will be abandoned. So we check the powershell.IsChild first to make sure
                // not to access the property "Runspace" in this case - powershell.isChild will be set to true only in this case.
                if (!powershell.IsChild)
                    CheckScriptCallOnRemoteRunspace(remoteRunspace);
                    // TabExpansion2 script is not available prior to PSv3.
                    if (remoteRunspace.GetCapabilities().Equals(Runspaces.RunspaceCapability.Default))
            return CallScriptWithStringParameterSet(input, cursorIndex, options, powershell);
        /// <param name="ast">The ast for pre-parsed input.</param>
        /// <param name="tokens"></param>
        /// <param name="cursorPosition"></param>
        public static CommandCompletion CompleteInput(Ast ast, Token[] tokens, IScriptPosition cursorPosition, Hashtable options, PowerShell powershell)
            if (cursorPosition == null)
                throw PSTraceSource.NewArgumentNullException(nameof(cursorPosition));
                return CompleteInputInDebugger(ast, tokens, cursorPosition, options, debugger);
                    // When calling the TabExpansion2 script, the input should be the whole script text
                    string input = ast.Extent.Text;
                    int cursorIndex = ((InternalScriptPosition)cursorPosition).Offset;
            return CallScriptWithAstParameterSet(ast, tokens, cursorPosition, options, powershell);
        /// Get the next result, moving forward or backward.  Supports wraparound, so if there are any results at all,
        /// this method will never fail and never return null.
        /// <param name="forward">True if we should move forward through the list, false if backwards.</param>
        /// <returns>The next completion result, or null if no results.</returns>
        public CompletionResult GetNextResult(bool forward)
            CompletionResult result = null;
            var count = CompletionMatches.Count;
            if (count > 0)
                CurrentMatchIndex += forward ? 1 : -1;
                if (CurrentMatchIndex >= count)
                    CurrentMatchIndex = 0;
                else if (CurrentMatchIndex < 0)
                    CurrentMatchIndex = count - 1;
                result = CompletionMatches[CurrentMatchIndex];
        /// Command completion while in debug break mode.
        /// <param name="debugger">Current debugger.</param>
        internal static CommandCompletion CompleteInputInDebugger(string input, int cursorIndex, Hashtable options, Debugger debugger)
                throw PSTraceSource.NewArgumentNullException(nameof(debugger));
            Command cmd = new Command("TabExpansion2");
            cmd.Parameters.Add("InputScript", input);
            cmd.Parameters.Add("CursorColumn", cursorIndex);
            cmd.Parameters.Add("Options", options);
            return ProcessCompleteInputCommand(cmd, debugger);
        /// <returns>Command completion.</returns>
        internal static CommandCompletion CompleteInputInDebugger(Ast ast, Token[] tokens, IScriptPosition cursorPosition, Hashtable options, Debugger debugger)
            // For remote debugging just pass string input.
            if ((debugger is RemoteDebugger) || debugger.IsPushed)
            cmd.Parameters.Add("Ast", ast);
            cmd.Parameters.Add("Tokens", tokens);
            cmd.Parameters.Add("PositionOfCursor", cursorPosition);
        private static CommandCompletion ProcessCompleteInputCommand(
            Command cmd,
            Debugger debugger)
            PSCommand command = new PSCommand(cmd);
            debugger.ProcessCommand(command, output);
                var commandCompletion = output[0].BaseObject as CommandCompletion;
                if (commandCompletion != null)
                    return commandCompletion;
        private static void CheckScriptCallOnRemoteRunspace(RemoteRunspace remoteRunspace)
            var remoteRunspaceInternal = remoteRunspace.RunspacePool.RemoteRunspacePoolInternal;
            if (remoteRunspaceInternal != null)
                var transportManager = remoteRunspaceInternal.DataStructureHandler.TransportManager;
                if (transportManager != null && transportManager.TypeTable == null)
                    // The remote runspace was created without a TypeTable instance.
                    // The tab completion results cannot be deserialized if the TypeTable is not available
                    throw PSTraceSource.NewInvalidOperationException(TabCompletionStrings.CannotDeserializeTabCompletionResult);
        private static CommandCompletion CallScriptWithStringParameterSet(string input, int cursorIndex, Hashtable options, PowerShell powershell)
                powershell.Commands.Clear();
                powershell.AddCommand("TabExpansion2")
                    .AddArgument(input)
                    .AddArgument(cursorIndex)
                    .AddArgument(options);
                var results = powershell.Invoke();
                if (results.Count == 1)
                    var result = PSObject.Base(results[0]);
                    var commandCompletion = result as CommandCompletion;
        private static CommandCompletion CallScriptWithAstParameterSet(Ast ast, Token[] tokens, IScriptPosition cursorPosition, Hashtable options, PowerShell powershell)
                    .AddArgument(ast)
                    .AddArgument(tokens)
                    .AddArgument(cursorPosition)
        // This is the start of the real implementation of autocomplete/intellisense/tab completion
        private static CommandCompletion CompleteInputImpl(Ast ast, Token[] tokens, IScriptPosition positionOfCursor, Hashtable options)
            // We could start collecting telemetry at a later date.
            // We will leave the #if to remind us that we did this once.
            using (var powershell = PowerShell.Create(RunspaceMode.CurrentRunspace))
                int replacementIndex = -1;
                int replacementLength = -1;
                List<CompletionResult> results = null;
                    // If we were invoked from TabExpansion2, we want to "remove" TabExpansion2 and anything it calls
                    // from our results.  We do this by faking out the session so that TabExpansion2 isn't anywhere to be found.
                    SessionStateScope scopeToRestore;
                    if (context.CurrentCommandProcessor is not null
                        && context.CurrentCommandProcessor.Command.CommandInfo.Name.Equals("TabExpansion2", StringComparison.OrdinalIgnoreCase)
                        && context.CurrentCommandProcessor.UseLocalScope
                        && context.EngineSessionState.CurrentScope.Parent is not null)
                        scopeToRestore = context.EngineSessionState.CurrentScope;
                        context.EngineSessionState.CurrentScope = scopeToRestore.Parent;
                        scopeToRestore = null;
                        var completionAnalysis = new CompletionAnalysis(ast, tokens, positionOfCursor, options);
                        results = completionAnalysis.GetResults(powershell, out replacementIndex, out replacementLength);
                        if (scopeToRestore != null)
                            context.EngineSessionState.CurrentScope = scopeToRestore;
                var completionResults = results ?? EmptyCompletionResult;
                // no telemetry here. We don't capture tab completion performance.
                TelemetryAPI.ReportTabCompletionTelemetry(sw.ElapsedMilliseconds, completionResults.Count,
                    completionResults.Count > 0 ? completionResults[0].ResultType : CompletionResultType.Text);
                return new CommandCompletion(
                    new Collection<CompletionResult>(completionResults),
                    -1,
                    replacementIndex,
                    replacementLength);

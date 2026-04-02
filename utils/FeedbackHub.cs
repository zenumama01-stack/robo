namespace System.Management.Automation.Subsystem.Feedback
    /// The class represents a result from a feedback provider.
    public class FeedbackResult
        /// Gets the Id of the feedback provider.
        /// Gets the name of the feedback provider.
        /// Gets the feedback item.
        public FeedbackItem Item { get; }
        internal FeedbackResult(Guid id, string name, FeedbackItem item)
            Item = item;
    /// Provides a set of feedbacks for given input.
    public static class FeedbackHub
        /// Collect the feedback from registered feedback providers using the default timeout.
        public static List<FeedbackResult>? GetFeedback(Runspace runspace)
            return GetFeedback(runspace, millisecondsTimeout: 1000);
        /// Collect the feedback from registered feedback providers using the specified timeout.
        public static List<FeedbackResult>? GetFeedback(Runspace runspace, int millisecondsTimeout)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(millisecondsTimeout);
            if (runspace is not LocalRunspace localRunspace)
            var providers = SubsystemManager.GetSubsystems<IFeedbackProvider>();
            if (providers.Count is 0)
            ExecutionContext executionContext = localRunspace.ExecutionContext;
            bool questionMarkValue = executionContext.QuestionMarkVariableValue;
            // The command line would have run successfully in most cases during an interactive use of the shell.
            // So, we do a quick check to see whether we can skip proceeding, so as to avoid unneeded allocations
            // from the 'TryGetFeedbackContext' call below.
            if (questionMarkValue && CanSkip(providers))
            // Get the last history item
            HistoryInfo[] histories = localRunspace.History.GetEntries(id: 0, count: 1, newest: true);
            if (histories.Length is 0)
            // Try creating the feedback context object.
            if (!TryGetFeedbackContext(executionContext, questionMarkValue, histories[0], out FeedbackContext? feedbackContext))
            int count = providers.Count;
            IFeedbackProvider? generalFeedback = null;
            List<Task<FeedbackResult?>>? tasks = null;
            CancellationTokenSource? cancellationSource = null;
            Func<object?, FeedbackResult?>? callBack = null;
            foreach (IFeedbackProvider provider in providers)
                if (!provider.Trigger.HasFlag(feedbackContext.Trigger))
                if (provider is GeneralCommandErrorFeedback)
                    // This built-in feedback provider needs to run on the target Runspace.
                    generalFeedback = provider;
                if (tasks is null)
                    tasks = new List<Task<FeedbackResult?>>(capacity: count);
                    cancellationSource = new CancellationTokenSource();
                    callBack = GetCallBack(feedbackContext, cancellationSource);
                // Other feedback providers will run on background threads in parallel.
                tasks.Add(Task.Factory.StartNew(
                    callBack!,
                    cancellationSource!.Token,
                    TaskCreationOptions.DenyChildAttach,
                    TaskScheduler.Default));
            Task<Task>? waitTask = null;
            if (tasks is not null)
                waitTask = Task.WhenAny(
                    Task.WhenAll(tasks),
                    Task.Delay(millisecondsTimeout, cancellationSource!.Token));
            List<FeedbackResult>? resultList = null;
            if (generalFeedback is not null)
                FeedbackResult? builtInResult = GetBuiltInFeedback(generalFeedback, localRunspace, feedbackContext, questionMarkValue);
                if (builtInResult is not null)
                    resultList ??= new List<FeedbackResult>(count);
                    resultList.Add(builtInResult);
            if (waitTask is not null)
                    waitTask.Wait();
                    cancellationSource!.Cancel();
                    foreach (Task<FeedbackResult?> task in tasks!)
                        if (task.IsCompletedSuccessfully)
                            FeedbackResult? result = task.Result;
                                resultList.Add(result);
                    cancellationSource!.Dispose();
        private static bool CanSkip(IEnumerable<IFeedbackProvider> providers)
            bool canSkip = true;
                if (provider.Trigger.HasFlag(FeedbackTrigger.Success))
                    canSkip = false;
            return canSkip;
        private static FeedbackResult? GetBuiltInFeedback(
            IFeedbackProvider builtInFeedback,
            LocalRunspace localRunspace,
            FeedbackContext feedbackContext,
            bool questionMarkValue)
            bool changedDefault = false;
            Runspace? oldDefault = Runspace.DefaultRunspace;
                if (oldDefault != localRunspace)
                    changedDefault = true;
                    Runspace.DefaultRunspace = localRunspace;
                FeedbackItem? item = builtInFeedback.GetFeedback(feedbackContext, CancellationToken.None);
                if (item is not null)
                    return new FeedbackResult(builtInFeedback.Id, builtInFeedback.Name, item);
                if (changedDefault)
                // Restore $? for the target Runspace.
                localRunspace.ExecutionContext.QuestionMarkVariableValue = questionMarkValue;
        private static bool TryGetFeedbackContext(
            ExecutionContext executionContext,
            bool questionMarkValue,
            HistoryInfo lastHistory,
            [NotNullWhen(true)] out FeedbackContext? feedbackContext)
            feedbackContext = null;
            Ast ast = Parser.ParseInput(lastHistory.CommandLine, out Token[] tokens, out _);
            FeedbackTrigger trigger;
            ErrorRecord? lastError = null;
            if (IsPureComment(tokens))
                // Don't trigger anything in this case.
            else if (questionMarkValue)
                trigger = FeedbackTrigger.Success;
            else if (TryGetLastError(executionContext, lastHistory, out lastError))
                trigger = lastError.FullyQualifiedErrorId is "CommandNotFoundException"
                    ? FeedbackTrigger.CommandNotFound
                    : FeedbackTrigger.Error;
            PathInfo cwd = executionContext.SessionState.Path.CurrentLocation;
            feedbackContext = new(trigger, ast, tokens, cwd, lastError);
        private static bool IsPureComment(Token[] tokens)
            return tokens.Length is 2 && tokens[0].Kind is TokenKind.Comment && tokens[1].Kind is TokenKind.EndOfInput;
        private static bool TryGetLastError(ExecutionContext context, HistoryInfo lastHistory, [NotNullWhen(true)] out ErrorRecord? lastError)
            lastError = null;
            ArrayList errorList = (ArrayList)context.DollarErrorVariable;
            lastError = errorList[0] as ErrorRecord;
            if (lastError is null && errorList[0] is RuntimeException rtEx)
                lastError = rtEx.ErrorRecord;
            if (lastError?.InvocationInfo is null || lastError.InvocationInfo.HistoryId != lastHistory.Id)
        // A local helper function to avoid creating an instance of the generated delegate helper class
        // when no feedback provider is registered.
        private static Func<object?, FeedbackResult?> GetCallBack(
            CancellationTokenSource cancellationSource)
            return state =>
                var provider = (IFeedbackProvider)state!;
                var item = provider.GetFeedback(feedbackContext, cancellationSource.Token);
                return item is null ? null : new FeedbackResult(provider.Id, provider.Name, item);

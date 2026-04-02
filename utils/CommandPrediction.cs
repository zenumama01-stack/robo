namespace System.Management.Automation.Subsystem.Prediction
    /// The class represents the prediction result from a predictor.
    public sealed class PredictionResult
        /// Gets the Id of the predictor.
        /// Gets the name of the predictor.
        /// Gets the mini-session id that represents a specific invocation to the <see cref="ICommandPredictor.GetSuggestion"/> API of the predictor.
        /// When it's not specified, it's considered by a client that the predictor doesn't expect feedback.
        public uint? Session { get; }
        /// Gets the suggestions.
        public IReadOnlyList<PredictiveSuggestion> Suggestions { get; }
        internal PredictionResult(Guid id, string name, uint? session, List<PredictiveSuggestion> suggestions)
            Session = session;
            Suggestions = suggestions;
    /// Provides a set of possible predictions for given input.
    public static class CommandPrediction
        /// Collect the predictive suggestions from registered predictors using the default timeout.
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="ast">The <see cref="Ast"/> object from parsing the current command line input.</param>
        /// <param name="astTokens">The <see cref="Token"/> objects from parsing the current command line input.</param>
        /// <returns>A list of <see cref="PredictionResult"/> objects.</returns>
        public static Task<List<PredictionResult>?> PredictInputAsync(PredictionClient client, Ast ast, Token[] astTokens)
            return PredictInputAsync(client, ast, astTokens, millisecondsTimeout: 20);
        /// Collect the predictive suggestions from registered predictors using the specified timeout.
        /// <param name="millisecondsTimeout">The milliseconds to timeout.</param>
        public static async Task<List<PredictionResult>?> PredictInputAsync(PredictionClient client, Ast ast, Token[] astTokens, int millisecondsTimeout)
            var predictors = SubsystemManager.GetSubsystems<ICommandPredictor>();
            if (predictors.Count == 0)
            var context = new PredictionContext(ast, astTokens);
            var tasks = new Task<PredictionResult?>[predictors.Count];
            using var cancellationSource = new CancellationTokenSource();
            Func<object?, PredictionResult?> callBack = GetCallBack(client, context, cancellationSource);
            for (int i = 0; i < predictors.Count; i++)
                ICommandPredictor predictor = predictors[i];
                tasks[i] = Task.Factory.StartNew(
                    callBack,
                    predictor,
                    cancellationSource.Token,
                    TaskScheduler.Default);
            await Task.WhenAny(
                Task.Delay(millisecondsTimeout, cancellationSource.Token)).ConfigureAwait(false);
            cancellationSource.Cancel();
            var resultList = new List<PredictionResult>(predictors.Count);
            foreach (Task<PredictionResult?> task in tasks)
                    PredictionResult? result = task.Result;
            // when no predictor is registered.
            static Func<object?, PredictionResult?> GetCallBack(
                PredictionClient client,
                PredictionContext context,
                    var predictor = (ICommandPredictor)state!;
                    SuggestionPackage pkg = predictor.GetSuggestion(client, context, cancellationSource.Token);
                    return pkg.SuggestionEntries?.Count > 0 ? new PredictionResult(predictor.Id, predictor.Name, pkg.Session, pkg.SuggestionEntries) : null;
        /// Allow registered predictors to do early processing when a command line is accepted.
        /// <param name="history">History command lines provided as references for prediction.</param>
        public static void OnCommandLineAccepted(PredictionClient client, IReadOnlyList<string> history)
            ArgumentNullException.ThrowIfNull(history);
            Action<ICommandPredictor>? callBack = null;
            foreach (ICommandPredictor predictor in predictors)
                if (predictor.CanAcceptFeedback(client, PredictorFeedbackKind.CommandLineAccepted))
                    callBack ??= GetCallBack(client, history);
                    ThreadPool.QueueUserWorkItem<ICommandPredictor>(callBack, predictor, preferLocal: false);
            // when no predictor is registered, or no registered predictor accepts this feedback.
            static Action<ICommandPredictor> GetCallBack(PredictionClient client, IReadOnlyList<string> history)
                return predictor => predictor.OnCommandLineAccepted(client, history);
        /// Allow registered predictors to know the execution result (success/failure) of the last accepted command line.
        /// <param name="commandLine">The last accepted command line.</param>
        /// <param name="success">Whether the execution of the last command line was successful.</param>
        public static void OnCommandLineExecuted(PredictionClient client, string commandLine, bool success)
                if (predictor.CanAcceptFeedback(client, PredictorFeedbackKind.CommandLineExecuted))
                    callBack ??= GetCallBack(client, commandLine, success);
            static Action<ICommandPredictor> GetCallBack(PredictionClient client, string commandLine, bool success)
                return predictor => predictor.OnCommandLineExecuted(client, commandLine, success);
        /// Send feedback to a predictor when one or more suggestions from it were displayed to the user.
        /// <param name="predictorId">The identifier of the predictor whose prediction result was accepted.</param>
        /// <param name="session">The mini-session where the displayed suggestions came from.</param>
        /// <param name="countOrIndex">
        /// When the value is greater than 0, it's the number of displayed suggestions from the list returned in <paramref name="session"/>, starting from the index 0.
        /// When the value is less than or equal to 0, it means a single suggestion from the list got displayed, and the index is the absolute value.
        public static void OnSuggestionDisplayed(PredictionClient client, Guid predictorId, uint session, int countOrIndex)
                if (predictor.Id == predictorId)
                    if (predictor.CanAcceptFeedback(client, PredictorFeedbackKind.SuggestionDisplayed))
                        Action<ICommandPredictor> callBack = GetCallBack(client, session, countOrIndex);
            static Action<ICommandPredictor> GetCallBack(PredictionClient client, uint session, int countOrIndex)
                return predictor => predictor.OnSuggestionDisplayed(client, session, countOrIndex);
        /// Send feedback to a predictor when a suggestion from it was accepted.
        /// <param name="session">The mini-session where the accepted suggestion came from.</param>
        /// <param name="suggestionText">The accepted suggestion text.</param>
        public static void OnSuggestionAccepted(PredictionClient client, Guid predictorId, uint session, string suggestionText)
            ArgumentException.ThrowIfNullOrEmpty(suggestionText);
                    if (predictor.CanAcceptFeedback(client, PredictorFeedbackKind.SuggestionAccepted))
                        Action<ICommandPredictor> callBack = GetCallBack(client, session, suggestionText);
            static Action<ICommandPredictor> GetCallBack(PredictionClient client, uint session, string suggestionText)
                return predictor => predictor.OnSuggestionAccepted(client, session, suggestionText);

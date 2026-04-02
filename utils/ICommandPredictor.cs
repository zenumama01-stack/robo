    /// Interface for implementing a predictor plugin.
    public interface ICommandPredictor : ISubsystem
        /// Default implementation. No function is required for a predictor.
        /// Get the predictive suggestions. It indicates the start of a suggestion rendering session.
        /// <param name="context">The <see cref="PredictionContext"/> object to be used for prediction.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the prediction.</param>
        /// <returns>An instance of <see cref="SuggestionPackage"/>.</returns>
        SuggestionPackage GetSuggestion(PredictionClient client, PredictionContext context, CancellationToken cancellationToken);
        /// Gets a value indicating whether the predictor accepts a specific kind of feedback.
        /// <param name="feedback">A specific type of feedback.</param>
        /// <returns>True or false, to indicate whether the specific feedback is accepted.</returns>
        bool CanAcceptFeedback(PredictionClient client, PredictorFeedbackKind feedback) => false;
        /// One or more suggestions provided by the predictor were displayed to the user.
        void OnSuggestionDisplayed(PredictionClient client, uint session, int countOrIndex) { }
        /// The suggestion provided by the predictor was accepted.
        /// <param name="session">Represents the mini-session where the accepted suggestion came from.</param>
        /// <param name="acceptedSuggestion">The accepted suggestion text.</param>
        void OnSuggestionAccepted(PredictionClient client, uint session, string acceptedSuggestion) { }
        /// A command line was accepted to execute.
        /// The predictor can start processing early as needed with the latest history.
        void OnCommandLineAccepted(PredictionClient client, IReadOnlyList<string> history) { }
        /// A command line was done execution.
        /// <param name="success">Shows whether the execution was successful.</param>
        void OnCommandLineExecuted(PredictionClient client, string commandLine, bool success) { }
    /// Kinds of feedback a predictor can choose to accept.
    public enum PredictorFeedbackKind
        /// Feedback when one or more suggestions are displayed to the user.
        SuggestionDisplayed,
        /// Feedback when a suggestion is accepted by the user.
        SuggestionAccepted,
        /// Feedback when a command line is accepted by the user.
        CommandLineAccepted,
        /// Feedback when the accepted command line finishes its execution.
        CommandLineExecuted,
    /// Kinds of prediction clients.
    public enum PredictionClientKind
        /// A terminal client, representing the command-line experience.
        Terminal,
        /// An editor client, representing the editor experience.
        Editor,
    /// The class represents a client that interacts with predictors.
    public sealed class PredictionClient
        /// Gets the client name.
        /// Gets the client kind.
        public PredictionClientKind Kind { get; }
        /// It returns null if there is no default Runspace or if the default is a remote Runspace.
        public PathInfo? CurrentLocation { get; set; }
        /// Initializes a new instance of the <see cref="PredictionClient"/> class.
        /// <param name="name">Name of the interactive client.</param>
        /// <param name="kind">Kind of the interactive client.</param>
        public PredictionClient(string name, PredictionClientKind kind)
    /// Context information about the user input.
    public sealed class PredictionContext
        /// Gets the abstract syntax tree (AST) generated from parsing the user input.
        public Ast InputAst { get; }
        /// Gets the tokens generated from parsing the user input.
        public IReadOnlyList<Token> InputTokens { get; }
        /// Gets the cursor position, which is assumed always at the end of the input line.
        public IScriptPosition CursorPosition { get; }
        /// Gets the token at the cursor.
        public Token? TokenAtCursor { get; }
        /// Gets all ASTs that are related to the cursor position,
        /// which is assumed always at the end of the input line.
        public IReadOnlyList<Ast> RelatedAsts { get; }
        /// Initializes a new instance of the <see cref="PredictionContext"/> class from the AST and tokens that represent the user input.
        /// <param name="inputAst">The <see cref="Ast"/> object from parsing the current command line input.</param>
        /// <param name="inputTokens">The <see cref="Token"/> objects from parsing the current command line input.</param>
        public PredictionContext(Ast inputAst, Token[] inputTokens)
            ArgumentNullException.ThrowIfNull(inputAst);
            ArgumentNullException.ThrowIfNull(inputTokens);
            var cursor = inputAst.Extent.EndScriptPosition;
            var astContext = CompletionAnalysis.ExtractAstContext(inputAst, inputTokens, cursor);
            InputAst = inputAst;
            InputTokens = inputTokens;
            CursorPosition = cursor;
            TokenAtCursor = astContext.TokenAtCursor;
            RelatedAsts = astContext.RelatedAsts;
        /// Creates a context instance from the user input line.
        /// <param name="input">The user input.</param>
        /// <returns>A <see cref="PredictionContext"/> object.</returns>
        public static PredictionContext Create(string input)
            ArgumentException.ThrowIfNullOrEmpty(input);
            Ast ast = Parser.ParseInput(input, out Token[] tokens, out _);
            return new PredictionContext(ast, tokens);
    /// The class represents a predictive suggestion generated by a predictor.
    public sealed class PredictiveSuggestion
        /// Gets the suggestion.
        public string SuggestionText { get; }
        /// Gets the tooltip of the suggestion.
        public string? ToolTip { get; }
        /// Initializes a new instance of the <see cref="PredictiveSuggestion"/> class.
        /// <param name="suggestion">The predictive suggestion text.</param>
        public PredictiveSuggestion(string suggestion)
            : this(suggestion, toolTip: null)
        /// <param name="toolTip">The tooltip of the suggestion.</param>
        public PredictiveSuggestion(string suggestion, string? toolTip)
            ArgumentException.ThrowIfNullOrEmpty(suggestion);
            SuggestionText = suggestion;
            ToolTip = toolTip;
    /// A package returned from <see cref="ICommandPredictor.GetSuggestion"/>.
    public struct SuggestionPackage
        /// Gets the mini-session that represents a specific invocation to <see cref="ICommandPredictor.GetSuggestion"/>.
        /// Gets the suggestion entries returned from that mini-session.
        public List<PredictiveSuggestion>? SuggestionEntries { get; }
        /// Initializes a new instance of the <see cref="SuggestionPackage"/> struct without providing a session id.
        /// Note that, when a session id is not specified, it's considered by a client that the predictor doesn't expect feedback.
        /// <param name="suggestionEntries">The suggestions to return.</param>
        public SuggestionPackage(List<PredictiveSuggestion> suggestionEntries)
            Requires.NotNullOrEmpty(suggestionEntries, nameof(suggestionEntries));
            Session = null;
            SuggestionEntries = suggestionEntries;
        /// Initializes a new instance of the <see cref="SuggestionPackage"/> struct with the mini-session id and the suggestions.
        /// <param name="session">The mini-session where suggestions came from.</param>
        public SuggestionPackage(uint session, List<PredictiveSuggestion> suggestionEntries)

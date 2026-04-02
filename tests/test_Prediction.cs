    public class MyPredictor : ICommandPredictor
        public List<string> History { get; }
        public List<string> Results { get; }
        public List<string> AcceptedSuggestions { get; }
        public List<string> DisplayedSuggestions { get; }
        public static readonly MyPredictor SlowPredictor, FastPredictor;
        static MyPredictor()
            SlowPredictor = new MyPredictor(
                "Test Predictor #1",
                "Description for #1 predictor.",
            FastPredictor = new MyPredictor(
                "Test Predictor #2",
                "Description for #2 predictor.",
                delay: false);
        private MyPredictor(Guid id, string name, string description, bool delay)
            History = new List<string>();
            Results = new List<string>();
            AcceptedSuggestions = new List<string>();
            DisplayedSuggestions = new List<string>();
            History.Clear();
            Results.Clear();
            AcceptedSuggestions.Clear();
            DisplayedSuggestions.Clear();
        #region "Interface implementation"
        bool ICommandPredictor.CanAcceptFeedback(PredictionClient client, PredictorFeedbackKind feedback) => true;
        public SuggestionPackage GetSuggestion(PredictionClient client, PredictionContext context, CancellationToken cancellationToken)
                Thread.Sleep(3000);
            // You can get the user input from the AST.
            var userInput = context.InputAst.Extent.Text;
            var entries = new List<PredictiveSuggestion>
                new PredictiveSuggestion($"'{userInput}' from '{client.Name}' - TEST-1 from {Name}"),
                new PredictiveSuggestion($"'{userInput}' from '{client.Name}' - TeSt-2 from {Name}"),
            return new SuggestionPackage(56, entries);
        public void OnSuggestionDisplayed(PredictionClient client, uint session, int countOrIndex)
            DisplayedSuggestions.Add($"{client.Name}-{session}-{countOrIndex}");
        public void OnSuggestionAccepted(PredictionClient client, uint session, string acceptedSuggestion)
            AcceptedSuggestions.Add($"{client.Name}-{session}-{acceptedSuggestion}");
        public void OnCommandLineAccepted(PredictionClient client, IReadOnlyList<string> history)
            foreach (string item in history)
                History.Add($"{client.Name}-{item}");
        public void OnCommandLineExecuted(PredictionClient client, string commandLine, bool success)
            Results.Add($"{client.Name}-{commandLine}-{success}");
    public static class CommandPredictionTests
        private const string Client = "PredictionTest";
        private const uint Session = 56;
        private static readonly PredictionClient predClient = new(Client, PredictionClientKind.Terminal);
        public static void PredictInput()
            const string Input = "Hello world";
            MyPredictor slow = MyPredictor.SlowPredictor;
            MyPredictor fast = MyPredictor.FastPredictor;
            Ast ast = Parser.ParseInput(Input, out Token[] tokens, out _);
            // Returns null when no predictor implementation registered
            List<PredictionResult> results = CommandPrediction.PredictInputAsync(predClient, ast, tokens).Result;
            Assert.Null(results);
                // Register 2 predictor implementations
                SubsystemManager.RegisterSubsystem<ICommandPredictor, MyPredictor>(slow);
                SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, fast);
                // Expect the results from 'fast' predictor only b/c the 'slow' one
                // cannot finish before the specified timeout.
                results = CommandPrediction.PredictInputAsync(predClient, ast, tokens, millisecondsTimeout: 1500).Result;
                PredictionResult res = results[0];
                Assert.Equal(fast.Id, res.Id);
                Assert.Equal(Session, res.Session);
                Assert.Equal(2, res.Suggestions.Count);
                Assert.Equal($"'{Input}' from '{Client}' - TEST-1 from {fast.Name}", res.Suggestions[0].SuggestionText);
                Assert.Equal($"'{Input}' from '{Client}' - TeSt-2 from {fast.Name}", res.Suggestions[1].SuggestionText);
                // Expect the results from both 'slow' and 'fast' predictors
                results = CommandPrediction.PredictInputAsync(predClient, ast, tokens, millisecondsTimeout: 4000).Result;
                Assert.Equal(2, results.Count);
                PredictionResult res1 = results[0];
                Assert.Equal(slow.Id, res1.Id);
                Assert.Equal(Session, res1.Session);
                Assert.Equal(2, res1.Suggestions.Count);
                Assert.Equal($"'{Input}' from '{Client}' - TEST-1 from {slow.Name}", res1.Suggestions[0].SuggestionText);
                Assert.Equal($"'{Input}' from '{Client}' - TeSt-2 from {slow.Name}", res1.Suggestions[1].SuggestionText);
                PredictionResult res2 = results[1];
                Assert.Equal(fast.Id, res2.Id);
                Assert.Equal(Session, res2.Session);
                Assert.Equal(2, res2.Suggestions.Count);
                Assert.Equal($"'{Input}' from '{Client}' - TEST-1 from {fast.Name}", res2.Suggestions[0].SuggestionText);
                Assert.Equal($"'{Input}' from '{Client}' - TeSt-2 from {fast.Name}", res2.Suggestions[1].SuggestionText);
                SubsystemManager.UnregisterSubsystem<ICommandPredictor>(slow.Id);
                SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, fast.Id);
        public static void Feedback()
            slow.Clear();
            fast.Clear();
                var history = new[] { "hello", "world" };
                var ids = new HashSet<Guid> { slow.Id, fast.Id };
                CommandPrediction.OnCommandLineAccepted(predClient, history);
                CommandPrediction.OnCommandLineExecuted(predClient, "last_input", true);
                CommandPrediction.OnSuggestionDisplayed(predClient, slow.Id, Session, 2);
                CommandPrediction.OnSuggestionDisplayed(predClient, fast.Id, Session, -1);
                CommandPrediction.OnSuggestionAccepted(predClient, slow.Id, Session, "Yeah");
                // The feedback calls are queued in thread pool, so let's wait a bit to make sure the calls are done.
                while (slow.History.Count == 0 || fast.History.Count == 0 ||
                       slow.Results.Count == 0 || fast.Results.Count == 0 ||
                       slow.DisplayedSuggestions.Count == 0 || fast.DisplayedSuggestions.Count == 0 ||
                       slow.AcceptedSuggestions.Count == 0)
                    Thread.Sleep(300);
                Assert.Equal(2, slow.History.Count);
                Assert.Equal($"{Client}-{history[0]}", slow.History[0]);
                Assert.Equal($"{Client}-{history[1]}", slow.History[1]);
                Assert.Equal(2, fast.History.Count);
                Assert.Equal($"{Client}-{history[0]}", fast.History[0]);
                Assert.Equal($"{Client}-{history[1]}", fast.History[1]);
                Assert.Single(slow.Results);
                Assert.Equal($"{Client}-last_input-True", slow.Results[0]);
                Assert.Single(fast.Results);
                Assert.Equal($"{Client}-last_input-True", fast.Results[0]);
                Assert.Single(slow.DisplayedSuggestions);
                Assert.Equal($"{Client}-{Session}-2", slow.DisplayedSuggestions[0]);
                Assert.Single(fast.DisplayedSuggestions);
                Assert.Equal($"{Client}-{Session}--1", fast.DisplayedSuggestions[0]);
                Assert.Single(slow.AcceptedSuggestions);
                Assert.Equal($"{Client}-{Session}-Yeah", slow.AcceptedSuggestions[0]);
                Assert.Empty(fast.AcceptedSuggestions);

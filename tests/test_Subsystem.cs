    public class MyInvalidSubsystem : ISubsystem
        public static readonly MyInvalidSubsystem Singleton;
        static MyInvalidSubsystem()
            Singleton = new MyInvalidSubsystem(Guid.NewGuid());
        private MyInvalidSubsystem(Guid id)
        public string Name => "Invalid";
        public string Description => "An invalid subsystem implementation";
        public Dictionary<string, string> FunctionsToDefine => null;
    public class MyCompositeSubsystem : ICommandPredictor, IFeedbackProvider
        public static readonly MyCompositeSubsystem Singleton;
        static MyCompositeSubsystem()
            Singleton = new MyCompositeSubsystem(Guid.NewGuid());
        private MyCompositeSubsystem(Guid id)
        public string Name => "Composite";
        public string Description => "A composite implementation that serves as both a feedback provider and a command predictor.";
        Dictionary<string, string> ISubsystem.FunctionsToDefine => null;
        #region IFeedbackProvider
        public FeedbackItem GetFeedback(FeedbackContext context, CancellationToken token) => new FeedbackItem("nothing", null);
        #region ICommandPredictor
        public SuggestionPackage GetSuggestion(PredictionClient client, PredictionContext context, CancellationToken cancellationToken) => default;
    public static class SubsystemTests
        private static readonly MyPredictor predictor1, predictor2;
        static SubsystemTests()
            predictor1 = MyPredictor.FastPredictor;
            predictor2 = MyPredictor.SlowPredictor;
        private static void VerifyCommandPredictorMetadata(SubsystemInfo ssInfo)
            Assert.Equal(SubsystemKind.CommandPredictor, ssInfo.Kind);
            Assert.Equal(typeof(ICommandPredictor), ssInfo.SubsystemType);
            Assert.True(ssInfo.AllowUnregistration);
            Assert.True(ssInfo.AllowMultipleRegistration);
            Assert.Empty(ssInfo.RequiredCmdlets);
            Assert.Empty(ssInfo.RequiredFunctions);
        private static void VerifyCrossPlatformDscMetadata(SubsystemInfo ssInfo)
            Assert.Equal(SubsystemKind.CrossPlatformDsc, ssInfo.Kind);
            Assert.Equal(typeof(ICrossPlatformDsc), ssInfo.SubsystemType);
            Assert.False(ssInfo.AllowMultipleRegistration);
        private static void VerifyFeedbackProviderMetadata(SubsystemInfo ssInfo)
            Assert.Equal(SubsystemKind.FeedbackProvider, ssInfo.Kind);
            Assert.Equal(typeof(IFeedbackProvider), ssInfo.SubsystemType);
        public static void GetSubsystemInfo()
            #region Predictor
            SubsystemInfo predictorInfo = SubsystemManager.GetSubsystemInfo(typeof(ICommandPredictor));
            SubsystemInfo predictorInfo2 = SubsystemManager.GetSubsystemInfo(SubsystemKind.CommandPredictor);
            Assert.Same(predictorInfo2, predictorInfo);
            VerifyCommandPredictorMetadata(predictorInfo);
            Assert.False(predictorInfo.IsRegistered);
            Assert.Empty(predictorInfo.Implementations);
            #region Feedback
            SubsystemInfo feedbackProviderInfo = SubsystemManager.GetSubsystemInfo(typeof(IFeedbackProvider));
            SubsystemInfo feedback2 = SubsystemManager.GetSubsystemInfo(SubsystemKind.FeedbackProvider);
            Assert.Same(feedback2, feedbackProviderInfo);
            VerifyFeedbackProviderMetadata(feedbackProviderInfo);
            Assert.True(feedbackProviderInfo.IsRegistered);
            Assert.Single(feedbackProviderInfo.Implementations);
            #region DSC
            SubsystemInfo crossPlatformDscInfo = SubsystemManager.GetSubsystemInfo(typeof(ICrossPlatformDsc));
            SubsystemInfo crossPlatformDscInfo2 = SubsystemManager.GetSubsystemInfo(SubsystemKind.CrossPlatformDsc);
            Assert.Same(crossPlatformDscInfo2, crossPlatformDscInfo);
            VerifyCrossPlatformDscMetadata(crossPlatformDscInfo);
            Assert.False(crossPlatformDscInfo.IsRegistered);
            Assert.Empty(crossPlatformDscInfo.Implementations);
            ReadOnlyCollection<SubsystemInfo> ssInfos = SubsystemManager.GetAllSubsystemInfo();
            Assert.Equal(3, ssInfos.Count);
            Assert.Same(ssInfos[0], predictorInfo);
            Assert.Same(ssInfos[1], crossPlatformDscInfo);
            Assert.Same(ssInfos[2], feedbackProviderInfo);
            ICommandPredictor predictorImpl = SubsystemManager.GetSubsystem<ICommandPredictor>();
            Assert.Null(predictorImpl);
            ReadOnlyCollection<ICommandPredictor> predictorImpls = SubsystemManager.GetSubsystems<ICommandPredictor>();
            Assert.Empty(predictorImpls);
            ReadOnlyCollection<IFeedbackProvider> feedbackImpls = SubsystemManager.GetSubsystems<IFeedbackProvider>();
            Assert.Single(feedbackImpls);
            ICrossPlatformDsc crossPlatformDscImpl = SubsystemManager.GetSubsystem<ICrossPlatformDsc>();
            Assert.Null(crossPlatformDscImpl);
            ReadOnlyCollection<ICrossPlatformDsc> crossPlatformDscImpls = SubsystemManager.GetSubsystems<ICrossPlatformDsc>();
            Assert.Empty(crossPlatformDscImpls);
        public static void RegisterSubsystemExpectedFailures()
            Assert.Throws<ArgumentNullException>(
                paramName: "proxy",
                () => SubsystemManager.RegisterSubsystem<ICommandPredictor, MyPredictor>(null));
                () => SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, null));
            ArgumentException ex = Assert.Throws<ArgumentException>(
                () => SubsystemManager.RegisterSubsystem(SubsystemKind.CrossPlatformDsc, predictor1));
            Assert.Contains(nameof(ICrossPlatformDsc), ex.Message);
            ex = Assert.Throws<ArgumentException>(
                paramName: "kind",
                () => SubsystemManager.RegisterSubsystem((SubsystemKind)0, predictor1));
            Assert.Contains("0", ex.Message);
                () => SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor | SubsystemKind.CrossPlatformDsc, predictor1));
            Assert.Contains("3", ex.Message);
            // You cannot register the instance of a type that only implements 'ISubsystem'.
                () => SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, MyInvalidSubsystem.Singleton));
            Assert.Contains(nameof(ICommandPredictor), ex.Message);
                paramName: "subsystemType",
                () => SubsystemManager.RegisterSubsystem<ISubsystem, MyInvalidSubsystem>(MyInvalidSubsystem.Singleton));
            Assert.Contains(nameof(ISubsystem), ex.Message);
        public static void RegisterSubsystemForCompositeImplementation()
                SubsystemManager.RegisterSubsystem<ICommandPredictor, MyCompositeSubsystem>(MyCompositeSubsystem.Singleton);
                SubsystemManager.RegisterSubsystem(SubsystemKind.FeedbackProvider, MyCompositeSubsystem.Singleton);
                SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, MyCompositeSubsystem.Singleton.Id);
                SubsystemManager.UnregisterSubsystem<IFeedbackProvider>(MyCompositeSubsystem.Singleton.Id);
        public static void RegisterSubsystem()
                // Register 'predictor1'
                SubsystemManager.RegisterSubsystem<ICommandPredictor, MyPredictor>(predictor1);
                // Now validate the SubsystemInfo of the 'ICommandPredictor' subsystem
                SubsystemInfo ssInfo = SubsystemManager.GetSubsystemInfo(typeof(ICommandPredictor));
                VerifyCommandPredictorMetadata(ssInfo);
                Assert.True(ssInfo.IsRegistered);
                Assert.Single(ssInfo.Implementations);
                // Now validate the 'ImplementationInfo'
                var implInfo = ssInfo.Implementations[0];
                Assert.Equal(predictor1.Id, implInfo.Id);
                Assert.Equal(predictor1.Name, implInfo.Name);
                Assert.Equal(predictor1.Description, implInfo.Description);
                Assert.Equal(SubsystemKind.CommandPredictor, implInfo.Kind);
                Assert.Same(typeof(MyPredictor), implInfo.ImplementationType);
                // Now validate the subsystem implementation itself.
                ICommandPredictor impl = SubsystemManager.GetSubsystem<ICommandPredictor>();
                Assert.Same(impl, predictor1);
                Assert.Null(impl.FunctionsToDefine);
                const string Client = "SubsystemTest";
                var predClient = new PredictionClient(Client, PredictionClientKind.Terminal);
                var predCxt = PredictionContext.Create(Input);
                var results = impl.GetSuggestion(predClient, predCxt, CancellationToken.None);
                Assert.Equal($"'{Input}' from '{Client}' - TEST-1 from {impl.Name}", results.SuggestionEntries[0].SuggestionText);
                Assert.Equal($"'{Input}' from '{Client}' - TeSt-2 from {impl.Name}", results.SuggestionEntries[1].SuggestionText);
                // Now validate the all-subsystem-implementation collection.
                ReadOnlyCollection<ICommandPredictor> impls = SubsystemManager.GetSubsystems<ICommandPredictor>();
                Assert.Single(impls);
                Assert.Same(predictor1, impls[0]);
                // Register 'predictor2'
                SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, predictor2);
                Assert.Equal(2, ssInfo.Implementations.Count);
                // Now validate the new 'ImplementationInfo'
                implInfo = ssInfo.Implementations[1];
                Assert.Equal(predictor2.Id, implInfo.Id);
                Assert.Equal(predictor2.Name, implInfo.Name);
                Assert.Equal(predictor2.Description, implInfo.Description);
                // Now validate the new subsystem implementation.
                impl = SubsystemManager.GetSubsystem<ICommandPredictor>();
                Assert.Same(impl, predictor2);
                impls = SubsystemManager.GetSubsystems<ICommandPredictor>();
                Assert.Equal(2, impls.Count);
                Assert.Same(predictor2, impls[1]);
                SubsystemManager.UnregisterSubsystem<ICommandPredictor>(predictor1.Id);
                SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, predictor2.Id);
        public static void UnregisterSubsystem()
            // Exception expected when no implementation is registered
            Assert.Throws<InvalidOperationException>(() => SubsystemManager.UnregisterSubsystem<ICommandPredictor>(predictor1.Id));
            // Exception is expected when specified id cannot be found
            Assert.Throws<InvalidOperationException>(() => SubsystemManager.UnregisterSubsystem<ICommandPredictor>(Guid.NewGuid()));
            // Unregister 'predictor1'
            SubsystemInfo ssInfo = SubsystemManager.GetSubsystemInfo(SubsystemKind.CommandPredictor);
            Assert.Same(predictor2, impls[0]);
            // Unregister 'predictor2'
            Assert.False(ssInfo.IsRegistered);
            Assert.Empty(ssInfo.Implementations);
            Assert.Null(impl);
            Assert.Empty(impls);

    /// Exposes the Engine APIs for a particular instance of the engine.
    public class EngineIntrinsics
        /// Hide the default constructor since we always require an instance of ExecutionContext.
        private EngineIntrinsics()
                "This constructor should never be called. Only the constructor that takes an instance of ExecutionContext should be called.");
        /// The internal constructor for this object. It should be the only one that gets called.
        /// An instance of ExecutionContext that the APIs should work against.
        internal EngineIntrinsics(ExecutionContext context)
            ArgumentNullException.ThrowIfNull(context);
            _host = context.EngineHostInterface;
        /// Gets engine APIs to access the host.
                    _host != null,
                    "The only constructor for this class should always set the host field");
                return _host;
        /// Gets engine APIs to access the event manager.
                return _context.Events;
        /// Gets the engine APIs to access providers.
                return _context.EngineSessionState.InvokeProvider;
        /// Gets the engine APIs to access session state.
                return _context.EngineSessionState.PublicSessionState;
        /// Gets the engine APIs to invoke a command.
        public CommandInvocationIntrinsics InvokeCommand
            get { return _invokeCommand ??= new CommandInvocationIntrinsics(_context); }
        private readonly PSHost _host;
        private CommandInvocationIntrinsics _invokeCommand;

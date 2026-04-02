    /// Derives InternalCommand for Native Commands.
    internal sealed class NativeCommand : InternalCommand
        private NativeCommandProcessor _myCommandProcessor;
        internal NativeCommandProcessor MyCommandProcessor
            get { return _myCommandProcessor; }
            set { _myCommandProcessor = value; }
        /// Implement the stop functionality for native commands...
        internal override void DoStopProcessing()
                _myCommandProcessor?.StopProcessing();

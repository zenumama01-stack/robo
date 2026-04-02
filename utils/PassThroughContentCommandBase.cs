    /// The base class for the */content commands that also take
    /// a passthrough parameter.
    public class PassThroughContentCommandBase : ContentCommandBase
        /// Gets or sets the passthrough parameter to the command.
                return _passThrough;
                _passThrough = value;
        /// Determines if the content returned from the provider should
        /// be passed through to the pipeline.
        /// Initializes a CmdletProviderContext instance to the current context of
        /// the command.
        /// A CmdletProviderContext instance initialized to the context of the current
        /// command.
        internal CmdletProviderContext GetCurrentContext()
            return currentCommandContext;

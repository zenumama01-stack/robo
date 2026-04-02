    /// Write message to message channel
    internal sealed class CimWriteMessage : CimBaseAction
        /// Channel id.
        #region Properties
        internal uint Channel { get; }
        internal string Message { get; }
        /// Initializes a new instance of the <see cref="CimWriteMessage"/> class.
        public CimWriteMessage(uint channel,
            string message)
            this.Channel = channel;
        /// Write message to the target channel
            switch ((CimWriteMessageChannel)Channel)
                case CimWriteMessageChannel.Verbose:
                    cmdlet.WriteVerbose(Message);
                case CimWriteMessageChannel.Warning:
                    cmdlet.WriteWarning(Message);
                case CimWriteMessageChannel.Debug:
                    cmdlet.WriteDebug(Message);

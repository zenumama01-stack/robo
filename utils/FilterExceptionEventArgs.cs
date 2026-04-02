    /// The EventArgs detailing the exception raised while
    /// evaluating the filter.
    public class FilterExceptionEventArgs : EventArgs
        /// Gets the Exception that was raised when filtering was
        /// evaluated.
        public Exception Exception
        /// Initializes a new instance of the FilterExceptionEventArgs
        /// class.
        /// <param name="exception">
        /// The Exception that was raised when filtering was evaluated.
        public FilterExceptionEventArgs(Exception exception)
            ArgumentNullException.ThrowIfNull(exception);

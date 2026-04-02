namespace System.Management.Automation.Remoting.WSMan
    /// This class channels WSMan server specific notifications to subscribers.
    /// One example is shutting down.
    public static class WSManServerChannelEvents
        /// Event raised when shutting down WSMan server.
        public static event EventHandler ShuttingDown;
        /// Event raised when active sessions in an endpoint are changed.
        public static event EventHandler<ActiveSessionsChangedEventArgs> ActiveSessionsChanged;
        /// Raising shutting down WSMan server event.
        internal static void RaiseShuttingDownEvent()
            ShuttingDown?.Invoke(null, EventArgs.Empty);
        /// Raising ActiveSessionsChanged event.
        internal static void RaiseActiveSessionsChangedEvent(ActiveSessionsChangedEventArgs eventArgs)
            ActiveSessionsChanged?.Invoke(null, eventArgs);
    /// Holds the event arguments when active sessions count changed.
    public sealed class ActiveSessionsChangedEventArgs : EventArgs
        /// Creates a new ActiveSessionsChangedEventArgs instance.
        /// <param name="activeSessionsCount"></param>
        public ActiveSessionsChangedEventArgs(int activeSessionsCount)
            ActiveSessionsCount = activeSessionsCount;
        /// ActiveSessionsCount.
        public int ActiveSessionsCount

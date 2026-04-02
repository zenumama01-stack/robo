        public const int INFINITE = -1;
        private static partial bool GetQueuedCompletionStatus(
            SafeIoCompletionPort CompletionPort,
            out int lpNumberOfBytesTransferred,
            out nint lpCompletionKey,
            out nint lpOverlapped,
            int dwMilliseconds);
        internal static bool GetQueuedCompletionStatus(
            SafeIoCompletionPort completionPort,
            int timeoutMilliseconds,
            out int status)
            return GetQueuedCompletionStatus(
                completionPort,
                out status,
                lpCompletionKey: out _,
                lpOverlapped: out _,
                timeoutMilliseconds);

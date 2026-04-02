        private static partial bool PostQueuedCompletionStatus(
            int lpNumberOfBytesTransferred,
            nint lpCompletionKey,
            nint lpOverlapped);
        internal static bool PostQueuedCompletionStatus(
            int status)
            return PostQueuedCompletionStatus(completionPort, status, nint.Zero, nint.Zero);

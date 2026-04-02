        internal sealed class SafeIoCompletionPort : SafeHandle
            public SafeIoCompletionPort() : base(invalidHandleValue: nint.Zero, ownsHandle: true) { }
            public override bool IsInvalid => handle == nint.Zero;
                => Windows.CloseHandle(handle);
        private static partial SafeIoCompletionPort CreateIoCompletionPort(
            nint FileHandle,
            nint ExistingCompletionPort,
            nint CompletionKey,
            int NumberOfConcurrentThreads);
        internal static SafeIoCompletionPort CreateIoCompletionPort()
            return CreateIoCompletionPort(
                FileHandle: -1,
                ExistingCompletionPort: nint.Zero,
                CompletionKey: nint.Zero,
                NumberOfConcurrentThreads: 1);

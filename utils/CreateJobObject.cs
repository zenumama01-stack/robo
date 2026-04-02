        internal sealed class SafeJobHandle : SafeHandle
            public SafeJobHandle() : base(invalidHandleValue: nint.Zero, ownsHandle: true) { }
        [LibraryImport("kernel32.dll", EntryPoint = "CreateJobObjectW", SetLastError = true)]
        private static partial SafeJobHandle CreateJobObject(
            nint lpJobAttributes,
            nint lpName);
        internal static SafeJobHandle CreateJobObject()
            => CreateJobObject(nint.Zero, nint.Zero);

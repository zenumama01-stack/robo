    internal static partial class Windows
        [LibraryImport("kernel32.dll", SetLastError = true)]
        internal static partial bool AssignProcessToJobObject(
            SafeJobHandle hJob,
            SafeProcessHandle hProcess);

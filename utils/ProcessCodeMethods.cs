    /// Helper functions for process info.
    public static class ProcessCodeMethods
        private const int InvalidProcessId = -1;
        internal static Process GetParent(this Process process)
                var pid = GetParentPid(process);
                if (pid == InvalidProcessId)
                var candidate = Process.GetProcessById(pid);
                // if the candidate was started later than process, the pid has been recycled
                return candidate.StartTime > process.StartTime ? null : candidate;
        /// CodeMethod for getting the parent process of a process.
        /// <returns>The parent process, or null if the parent is no longer running.</returns>
        public static object GetParentProcess(PSObject obj)
            var process = PSObject.Base(obj) as Process;
            return process?.GetParent();
        /// Returns the parent id of a process or -1 if it fails.
        /// <returns>The pid of the parent process.</returns>
        internal static int GetParentPid(Process process)
            return Platform.NonWindowsGetProcessParentPid(process.Id);
            Diagnostics.Assert(process != null, "Ensure process is not null before calling");
            Interop.Windows.PROCESS_BASIC_INFORMATION pbi;
            var res = Interop.Windows.NtQueryInformationProcess(process.Handle, 0, out pbi, Marshal.SizeOf<Interop.Windows.PROCESS_BASIC_INFORMATION>(), out size);
            return res != 0 ? InvalidProcessId : pbi.InheritedFromUniqueProcessId.ToInt32();

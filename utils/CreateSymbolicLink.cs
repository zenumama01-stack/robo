        internal enum SymbolicLinkFlags
            File = 0,
            Directory = 1,
            AllowUnprivilegedCreate = 2,
        [LibraryImport("api-ms-win-core-file-l2-1-0.dll", EntryPoint = "CreateSymbolicLinkW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        internal static partial bool CreateSymbolicLink(string name, string destination, SymbolicLinkFlags symbolicLinkFlags);

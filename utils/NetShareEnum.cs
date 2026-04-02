        internal const int MAX_PREFERRED_LENGTH = -1;
        internal const int STYPE_DISKTREE = 0;
        internal const int STYPE_MASK = 0x000000FF;
        [LibraryImport("Netapi32.dll", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial int NetShareEnum(
            string serverName,
            int level,
            out nint bufptr,
            int prefMaxLen,
            out uint entriesRead,
            out uint totalEntries,
            ref uint resumeHandle);

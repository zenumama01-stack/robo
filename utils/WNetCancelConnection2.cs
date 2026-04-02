        [LibraryImport("mpr.dll", EntryPoint = "WNetCancelConnection2W", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial int WNetCancelConnection2W(string driveName, int flags, [MarshalAs(UnmanagedType.Bool)] bool force);
        internal static int WNetCancelConnection2(string driveName, int flags, bool force)
                errorCode = WNetCancelConnection2W(driveName, flags, force: true);

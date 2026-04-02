        [LibraryImport("api-ms-win-core-file-l2-1-0.dll", EntryPoint = "CreateHardLinkW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        internal static partial bool CreateHardLink(string name, string existingFileName, nint securityAttributes);

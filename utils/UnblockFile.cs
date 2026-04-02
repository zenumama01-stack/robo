    /// <summary>Removes the Zone.Identifier stream from a file.</summary>
    [Cmdlet(VerbsSecurity.Unblock, "File", DefaultParameterSetName = "ByPath", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097033")]
    public sealed class UnblockFileCommand : PSCmdlet
        private const string MacBlockAttribute = "com.apple.quarantine";
        private const int RemovexattrFollowSymLink = 0;
        /// The path of the file to unblock.
        /// The literal path of the file to unblock.
        [Parameter(Mandatory = true, ParameterSetName = "ByLiteralPath", ValueFromPipelineByPropertyName = true)]
        /// Generate the type(s)
            if (string.Equals(this.ParameterSetName, "ByLiteralPath", StringComparison.OrdinalIgnoreCase))
                    if (IsValidFileForUnblocking(newPath))
                // Resolve paths
                        foreach (string currentFilepath in newPaths)
                            if (IsValidFileForUnblocking(currentFilepath))
                                pathsToProcess.Add(currentFilepath);
            // Unblock files
                if (ShouldProcess(path))
                        AlternateDataStreamUtilities.DeleteFileStream(path, "Zone.Identifier");
                        WriteError(new ErrorRecord(exception: e, errorId: "RemoveItemUnableToAccessFile", ErrorCategory.ResourceUnavailable, targetObject: path));
            if (Platform.IsLinux)
                string errorMessage = UnblockFileStrings.LinuxNotSupported;
                Exception e = new PlatformNotSupportedException(errorMessage);
                ThrowTerminatingError(new ErrorRecord(exception: e, errorId: "LinuxNotSupported", ErrorCategory.NotImplemented, targetObject: null));
                if (IsBlocked(path))
                    UInt32 result = RemoveXattr(path, MacBlockAttribute, RemovexattrFollowSymLink);
                        string errorMessage = string.Format(CultureInfo.CurrentUICulture, UnblockFileStrings.UnblockError, path);
                        Exception e = new InvalidOperationException(errorMessage);
                        WriteError(new ErrorRecord(exception: e, errorId: "UnblockError", ErrorCategory.InvalidResult, targetObject: path));
        /// IsValidFileForUnblocking is a helper method used to validate if
        /// the supplied file path has to be considered for unblocking.
        /// <param name="resolvedpath">File or directory path.</param>
        /// <returns>True is the supplied path is a
        /// valid file path or else false is returned.
        /// If the supplied path is a directory path then false is returned.</returns>
        private bool IsValidFileForUnblocking(string resolvedpath)
            bool isValidUnblockableFile = false;
            // Bug 501423 : silently ignore folders given that folders cannot have
            // alternate data streams attached to them (i.e. they're already unblocked).
            if (!System.IO.Directory.Exists(resolvedpath))
                if (!System.IO.File.Exists(resolvedpath))
                        new System.IO.FileNotFoundException(resolvedpath),
                        resolvedpath);
                    isValidUnblockableFile = true;
            return isValidUnblockableFile;
        private static bool IsBlocked(string path)
            const uint valueSize = 1024;
            IntPtr value = Marshal.AllocHGlobal((int)valueSize);
                var resultSize = GetXattr(path, MacBlockAttribute, value, valueSize, 0, RemovexattrFollowSymLink);
                return resultSize != -1;
                Marshal.FreeHGlobal(value);
        // Ansi means UTF8 on Unix
        // https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man2/RemoveXattr.2.html
        [DllImport("libc", SetLastError = true, EntryPoint = "removexattr", CharSet = CharSet.Ansi)]
        private static extern UInt32 RemoveXattr(string path, string name, int options);
        [DllImport("libc", EntryPoint = "getxattr", CharSet = CharSet.Ansi)]
        private static extern long GetXattr(
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            IntPtr value,
            ulong size,
            uint position,
            int options);

    /// Defines generic utilities and helper methods for PowerShell.
    internal static class PathUtils
        /// THE method for opening a file for writing.
        /// Should be used by all cmdlets that write to a file.
        /// <param name="cmdlet">Cmdlet that is opening the file (used mainly for error reporting).</param>
        /// <param name="filePath">Path to the file (as specified on the command line - this method will resolve the path).</param>
        /// <param name="encoding">Encoding (this method will convert the command line string to an Encoding instance).</param>
        /// <param name="defaultEncoding">If <see langword="true"/>, then we will use default .NET encoding instead of the encoding specified in <paramref name="encoding"/> parameter.</param>
        /// <param name="Append"></param>
        /// <param name="Force"></param>
        /// <param name="NoClobber"></param>
        /// <param name="fileStream">Result1: <see cref="FileStream"/> opened for writing.</param>
        /// <param name="streamWriter">Result2: <see cref="StreamWriter"/> (inherits from <see cref="TextWriter"/>) opened for writing.</param>
        /// <param name="readOnlyFileInfo">Result3: file info that should be used to restore file attributes after done with the file (<see langword="null"/> is this is not needed).</param>
        /// <param name="isLiteralPath">True if wildcard expansion should be bypassed.</param>
        internal static void MasterStreamOpen(
            string encoding,
            bool defaultEncoding,
            bool Append,
            bool Force,
            bool NoClobber,
            out FileStream fileStream,
            out StreamWriter streamWriter,
            out FileInfo readOnlyFileInfo,
            bool isLiteralPath
            Encoding resolvedEncoding = EncodingConversion.Convert(cmdlet, encoding);
            MasterStreamOpen(cmdlet, filePath, resolvedEncoding, defaultEncoding, Append, Force, NoClobber, out fileStream, out streamWriter, out readOnlyFileInfo, isLiteralPath);
        /// <param name="resolvedEncoding">Encoding (this method will convert the command line string to an Encoding instance).</param>
            Encoding resolvedEncoding,
            bool isLiteralPath)
            fileStream = null;
            streamWriter = null;
            string resolvedPath = ResolveFilePath(filePath, cmdlet, isLiteralPath);
                MasterStreamOpenImpl(
                    resolvedEncoding,
                    defaultEncoding,
                    out readOnlyFileInfo);
                ReportFileOpenFailure(cmdlet, resolvedPath, e);
                if (NoClobber && File.Exists(resolvedPath))
                    // This probably happened because the file already exists
                        e, "NoClobber", ErrorCategory.ResourceExists, resolvedPath);
                        "PathUtilsStrings",
                        "UtilityFileExistsNoClobber",
            string resolvedPath = ResolveFilePath(filePath, isLiteralPath);
                AddFileOpenErrorRecord(e);
                        PathUtilsStrings.UtilityFileExistsNoClobber,
                        "NoClobber");
                    e.Data[typeof(ErrorRecord)] = errorRecord;
            static void AddFileOpenErrorRecord(Exception e)
        /// <param name="resolvedPath">Path to the file (as specified on the command line - this method will resolve the path).</param>
        internal static void MasterStreamOpenImpl(
            string resolvedPath,
            out FileInfo readOnlyFileInfo)
            // variable to track file open mode
            // this is controlled by append/force parameters
            FileMode mode = FileMode.Create;
                mode = FileMode.Append;
            else if (NoClobber)
                // throw IOException if file exists
                mode = FileMode.CreateNew;
            if (Force && (Append || !NoClobber))
                if (File.Exists(resolvedPath))
                    FileInfo fInfo = new FileInfo(resolvedPath);
            // if the user knows what he/she is doing and uses "-Force" switch,
            // then we let more than 1 process write to the same file at the same time
            FileShare fileShare = Force ? FileShare.ReadWrite : FileShare.Read;
            // mode is controlled by force and ShouldContinue()
            fileStream = new FileStream(resolvedPath, mode, FileAccess.Write, fileShare);
            // create stream writer
            // NTRAID#Windows Out Of Band Releases-931008-2006/03/27
            // For some reason, calling this without specifying
            // the encoding is different from passing Encoding.Default.
            if (defaultEncoding)
                streamWriter = new StreamWriter(fileStream);
                streamWriter = new StreamWriter(fileStream, resolvedEncoding);
        internal static void ReportFileOpenFailure(Cmdlet cmdlet, string filePath, Exception e)
        internal static void ReportFileOpenFailure(string filePath, Exception e)
                errorRecord.Exception,
                errorRecord);
        internal static StreamReader OpenStreamReader(PSCmdlet command, string filePath, Encoding encoding, bool isLiteralPath)
            FileStream fileStream = OpenFileStream(filePath, command, isLiteralPath);
            return new StreamReader(fileStream, encoding);
        internal static FileStream OpenFileStream(string filePath, PSCmdlet command, bool isLiteralPath)
            string resolvedPath = PathUtils.ResolveFilePath(filePath, command, isLiteralPath);
                return new FileStream(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // These are the known exceptions for FileStream.ctor
                PathUtils.ReportFileOpenFailure(command, filePath, e);
                return null; // the line above will throw - silencing the compiler
        /// Resolve a user provided file name or path (including globbing characters)
        /// to a fully qualified file path, using the file system provider.
        internal static string ResolveFilePath(string filePath, PSCmdlet command)
            return ResolveFilePath(filePath, command, false);
        internal static string ResolveFilePath(string filePath, PSCmdlet command, bool isLiteralPath)
                List<string> filePaths = new List<string>();
                    filePaths.Add(command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath, out provider, out drive));
                    filePaths.AddRange(command.SessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider));
                if (!provider.NameEquals(command.Context.ProviderNames.FileSystem))
                    ReportWrongProviderType(command, provider.FullName);
                    ReportMultipleFilesNotSupported(command);
                if (filePaths.Count == 0)
                    ReportWildcardingFailure(command, filePath);
                CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(command);
                    command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
        internal static string ResolveFilePath(string filePath, bool isLiteralPath)
            SessionState sessionState = LocalPipeline.GetExecutionContextFromTLS()?.EngineSessionState?.PublicSessionState;
            if (sessionState is null)
                List<string> filePaths = new();
                    filePaths.Add(sessionState.Path.GetUnresolvedProviderPathFromPSPath(filePath, out provider, out drive));
                    filePaths.AddRange(sessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider));
                if (!provider.NameEquals(FileSystemProvider.ProviderName))
                    ReportWildcardingFailure(filePath);
        internal static void ReportWrongProviderType(Cmdlet cmdlet, string providerId)
            string msg = StringUtil.Format(PathUtilsStrings.OutFile_ReadWriteFileNotFileSystemProvider, providerId);
                PSTraceSource.NewInvalidOperationException(),
        internal static void ReportWrongProviderType(string providerId)
            PSInvalidOperationException exception = PSTraceSource.NewInvalidOperationException();
            exception.Data[typeof(ErrorRecord)] = errorRecord;
        internal static void ReportMultipleFilesNotSupported(Cmdlet cmdlet)
            string msg = StringUtil.Format(PathUtilsStrings.OutFile_MultipleFilesNotSupported);
                "ReadWriteMultipleFilesNotSupported",
        internal static void ReportMultipleFilesNotSupported()
        internal static void ReportWildcardingFailure(Cmdlet cmdlet, string filePath)
            string msg = StringUtil.Format(PathUtilsStrings.OutFile_DidNotResolveFile, filePath);
                new FileNotFoundException(),
        internal static void ReportWildcardingFailure(string filePath)
            FileNotFoundException exception = new();
        internal static DirectoryInfo CreateModuleDirectory(PSCmdlet cmdlet, string moduleNameOrPath, bool force)
            Dbg.Assert(cmdlet != null, "Caller should verify cmdlet != null");
            Dbg.Assert(!string.IsNullOrEmpty(moduleNameOrPath), "Caller should verify !string.IsNullOrEmpty(moduleNameOrPath)");
            DirectoryInfo directoryInfo = null;
                // Even if 'moduleNameOrPath' is a rooted path, 'ResolveRootedFilePath' may return null when the path doesn't exist yet,
                // or when it contains wildcards but cannot be resolved to a single path.
                string rootedPath = ModuleCmdletBase.ResolveRootedFilePath(moduleNameOrPath, cmdlet.Context);
                if (string.IsNullOrEmpty(rootedPath) && moduleNameOrPath.StartsWith('.'))
                    PathInfo currentPath = cmdlet.CurrentProviderLocation(cmdlet.Context.ProviderNames.FileSystem);
                    rootedPath = Path.Combine(currentPath.ProviderPath, moduleNameOrPath);
                    if (Path.IsPathRooted(moduleNameOrPath))
                        rootedPath = moduleNameOrPath;
                        string personalModuleRoot = ModuleIntrinsics.GetPersonalModulePath();
                        if (string.IsNullOrEmpty(personalModuleRoot))
                                    new ArgumentException(StringUtil.Format(PathUtilsStrings.ExportPSSession_ErrorModuleNameOrPath, moduleNameOrPath)),
                                    "ExportPSSession_ErrorModuleNameOrPath",
                                    cmdlet));
                        rootedPath = Path.Combine(personalModuleRoot, moduleNameOrPath);
                directoryInfo = new DirectoryInfo(rootedPath);
                if (directoryInfo.Exists)
                            CultureInfo.InvariantCulture, // directory name should be treated as culture-invariant
                            PathUtilsStrings.ExportPSSession_ErrorDirectoryExists,
                            directoryInfo.FullName);
                        ErrorDetails details = new ErrorDetails(errorMessage);
                            "ExportProxyCommand_OutputDirectoryExists",
                            directoryInfo);
                    directoryInfo.Create();
                    PathUtilsStrings.ExportPSSession_CannotCreateOutputDirectory,
                    moduleNameOrPath,
                    new ArgumentException(details.Message, e),
                    "ExportProxyCommand_CannotCreateOutputDirectory",
                    moduleNameOrPath);
            return directoryInfo;
        internal static DirectoryInfo CreateTemporaryDirectory()
            DirectoryInfo temporaryDirectory = new DirectoryInfo(Path.GetTempPath());
            DirectoryInfo moduleDirectory;
                moduleDirectory = new DirectoryInfo(
                    Path.Combine(
                        temporaryDirectory.FullName,
                            "tmp_{0}",
                            Path.GetRandomFileName())));
            } while (moduleDirectory.Exists);
            Directory.CreateDirectory(moduleDirectory.FullName);
            return new DirectoryInfo(moduleDirectory.FullName);
        internal static bool TryDeleteFile(string filepath)
            if (IO.File.Exists(filepath))
                    IO.File.Delete(filepath);
                    // file is in use on Windows
                    // user does not have permissions
        #region Helpers for long paths from .Net Runtime
        // Code here is copied from .NET's internal path helper implementation:
        // https://github.com/dotnet/runtime/blob/dcce0f56e10f5ac9539354b049341a2d7c0cdebf/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs
        // It has been left as a verbatim copy.
        /// Adds the extended path prefix (\\?\) if not already a device path, IF the path is not relative,
        /// AND the path is more than 259 characters. (> MAX_PATH + null). This will also insert the extended
        /// prefix if the path ends with a period or a space. Trailing periods and spaces are normally eaten
        /// away from paths during normalization, but if we see such a path at this point it should be
        /// normalized and has retained the final characters. (Typically from one of the *Info classes).
        /// <returns>File path (with extended prefix if the path is long path).</returns>
        [return: NotNullIfNotNull(nameof(path))]
        internal static string? EnsureExtendedPrefixIfNeeded(string? path)
            if (path != null && (path.Length >= MaxShortPath || EndsWithPeriodOrSpace(path)))
                return EnsureExtendedPrefix(path);
        internal static string EnsureExtendedPrefix(string path)
            if (IsPartiallyQualified(path) || IsDevice(path))
            // Given \\server\share in longpath becomes \\?\UNC\server\share
            if (path.StartsWith(UncPathPrefix, StringComparison.OrdinalIgnoreCase))
                return path.Insert(2, UncDevicePrefixToInsert);
            return ExtendedDevicePathPrefix + path;
        private const string ExtendedDevicePathPrefix = @"\\?\";
        private const string UncPathPrefix = @"\\";
        private const string UncDevicePrefixToInsert = @"?\UNC\";
        private const string UncExtendedPathPrefix = @"\\?\UNC\";
        private const string DevicePathPrefix = @"\\.\";
        private const int MaxShortPath = 260;
        // \\?\, \\.\, \??\
        private const int DevicePrefixLength = 4;
        private static bool EndsWithPeriodOrSpace(string? path)
            char c = path[path.Length - 1];
            return c == ' ' || c == '.';
        /// Returns true if the given character is a valid drive letter
        private static bool IsValidDriveChar(char value)
            return ((value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z'));
        private static bool IsDevice(string path)
            return IsExtended(path)
                    path.Length >= DevicePrefixLength
                    && IsDirectorySeparator(path[0])
                    && IsDirectorySeparator(path[1])
                    && (path[2] == '.' || path[2] == '?')
                    && IsDirectorySeparator(path[3])
        private static bool IsExtended(string path)
            return path.Length >= DevicePrefixLength
                && path[0] == '\\'
                && (path[1] == '\\' || path[1] == '?')
                && path[2] == '?'
                && path[3] == '\\';
        /// Returns true if the path specified is relative to the current drive or working directory.
        /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
        /// validation of the path (URIs will be returned as relative as a result).
        /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
        /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
        /// "C:a" is drive relative- meaning that it will be resolved against the current directory
        /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
        /// will not be used to modify the path).
        private static bool IsPartiallyQualified(string path)
            if (path.Length < 2)
                // It isn't fixed, it must be relative.  There is no way to specify a fixed
                // path with one character (or less).
            if (IsDirectorySeparator(path[0]))
                // There is no valid way to specify a relative path with two initial slashes or
                // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
                return !(path[1] == '?' || IsDirectorySeparator(path[1]));
            // The only way to specify a fixed path that doesn't begin with two slashes
            // is the drive, colon, slash format- i.e. C:\
            return !((path.Length >= 3)
                && (path[1] == Path.VolumeSeparatorChar)
                && IsDirectorySeparator(path[2])
                // To match old behavior we'll check the drive character for validity as the path is technically
                // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
                && IsValidDriveChar(path[0]));
        /// True if the given character is a directory separator.
        private static bool IsDirectorySeparator(char c)
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        #region Helpers for checking invalid paths using SearchValues
        /// Contains characters that are invalid in file names.
        private static readonly SearchValues<char> s_invalidFileNameChars
            = SearchValues.Create(Path.GetInvalidFileNameChars());
        /// Contains characters that are invalid in path names.
        private static readonly SearchValues<char> s_invalidPathChars
            = SearchValues.Create(Path.GetInvalidPathChars());
        /// Checks if the specified filename contains any characters that are invalid in file names.
        /// <param name="filename">The path to check.</param>
        /// <returns>True if the filename contains invalid file name characters, otherwise false.</returns>
        internal static bool ContainsInvalidFileNameChars(ReadOnlySpan<char> filename)
            => filename.ContainsAny(s_invalidFileNameChars);
        /// Checks if the specified path contains any characters that are invalid in path names.
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path contains invalid path characters, otherwise false.</returns>
        internal static bool ContainsInvalidPathChars(ReadOnlySpan<char> path)
            => path.ContainsAny(s_invalidPathChars);

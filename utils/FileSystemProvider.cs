    #region FileSystemProvider
    /// Defines the implementation of a File System Provider.  This provider
    /// allows for stateless namespace navigation of the file system.
    [CmdletProvider(FileSystemProvider.ProviderName, ProviderCapabilities.Credentials | ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    [OutputType(typeof(FileSecurity), ProviderCmdlet = ProviderCmdlet.SetAcl)]
    [OutputType(typeof(byte), typeof(string), ProviderCmdlet = ProviderCmdlet.GetContent)]
    [OutputType(typeof(FileInfo), ProviderCmdlet = ProviderCmdlet.GetItem)]
    [OutputType(typeof(FileInfo), typeof(DirectoryInfo), ProviderCmdlet = ProviderCmdlet.GetChildItem)]
    [OutputType(typeof(FileSecurity), typeof(DirectorySecurity), ProviderCmdlet = ProviderCmdlet.GetAcl)]
    [OutputType(typeof(bool), typeof(string), typeof(FileInfo), typeof(DirectoryInfo), ProviderCmdlet = ProviderCmdlet.GetItem)]
    [OutputType(typeof(bool), typeof(string), typeof(DateTime), typeof(System.IO.FileInfo), typeof(System.IO.DirectoryInfo), ProviderCmdlet = ProviderCmdlet.GetItemProperty)]
    [OutputType(typeof(string), typeof(System.IO.FileInfo), typeof(DirectoryInfo), ProviderCmdlet = ProviderCmdlet.NewItem)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This coupling is required")]
    public sealed partial class FileSystemProvider : NavigationCmdletProvider,
                                                     IContentCmdletProvider,
                                                     IPropertyCmdletProvider,
                                                     ISecurityDescriptorCmdletProvider,
                                                     ICmdletProviderSupportsHelp
        // 4MB gives the best results without spiking the resources on the remote connection for file transfers between pssessions.
        // NOTE: The script used to copy file data from session (PSCopyFromSessionHelper) has a
        // maximum fragment size value for security.  If FILETRANSFERSIZE changes make sure the
        // copy script will accommodate the new value.
        private const int FILETRANSFERSIZE = 4 * 1024 * 1024;
        private const int COPY_FILE_ACTIVITY_ID = 0;
        private const int REMOVE_FILE_ACTIVITY_ID = 0;
        // The name of the key in an exception's Data dictionary when attempting
        // to copy an item onto itself.
        private const string SelfCopyDataKey = "SelfCopy";
        /// using "FileSystemProvider" as the category.
        [Dbg.TraceSource("FileSystemProvider", "The namespace navigation provider for the file system")]
            Dbg.PSTraceSource.GetTracer("FileSystemProvider", "The namespace navigation provider for the file system");
        public const string ProviderName = "FileSystem";
        /// Initializes a new instance of the FileSystemProvider class. Since this
        /// object needs to be stateless, the constructor does nothing.
        public FileSystemProvider()
        private Collection<WildcardPattern> _excludeMatcher = null;
        /// Converts all / in the path to \
        /// The path to normalize.
        /// The path with all / normalized to \
        internal static string NormalizePath(string path)
            return GetCorrectCasedPath(path.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator));
        /// Get the correct casing for a path.  This method assumes it's being called by NormalizePath()
        /// so that the path is already normalized.
        /// The path to retrieve.
        /// The path with accurate casing if item exists, otherwise it returns path that was passed in.
        private static string GetCorrectCasedPath(string path)
            // Only apply to directories where there are issues with some tools if the casing
            // doesn't match the source like git
            if (Directory.Exists(path))
                string exactPath = string.Empty;
                int itemsToSkip = 0;
                if (Utils.PathIsUnc(path))
                    // With the Split method, a UNC path like \\server\share, we need to skip
                    // trying to enumerate the server and share, so skip the first two empty
                    // strings, then server, and finally share name.
                    itemsToSkip = 4;
                var items = path.Split(StringLiterals.DefaultPathSeparator);
                for (int i = 0; i < items.Length; i++)
                    if (itemsToSkip-- > 0)
                        // This handles the UNC server and share and 8.3 short path syntax
                        exactPath += items[i] + StringLiterals.DefaultPathSeparator;
                    else if (string.IsNullOrEmpty(exactPath))
                        // This handles the drive letter or / root path start
                        exactPath = items[i] + StringLiterals.DefaultPathSeparator;
                    else if (string.IsNullOrEmpty(items[i]) && i == items.Length - 1)
                        // This handles the trailing slash case
                        if (!exactPath.EndsWith(StringLiterals.DefaultPathSeparator))
                            exactPath += StringLiterals.DefaultPathSeparator;
                    else if (items[i].Contains('~'))
                        // This handles short path names
                        exactPath += StringLiterals.DefaultPathSeparator + items[i];
                        // Use GetFileSystemEntries to get the correct casing of this element
                            var entries = Directory.GetFileSystemEntries(exactPath, items[i]);
                            if (entries.Length > 0)
                                exactPath = entries[0];
                                // If previous call didn't return anything, something failed so we just return the path we were given
                            // If we can't enumerate, we stop and just return the original path
                return exactPath;
        /// Checks if the item exist at the specified path. if it exists then creates
        /// appropriate directoryinfo or fileinfo object.
        /// Refers to the item for which we are checking for existence and creating filesysteminfo object.
        /// <param name="isContainer">
        /// Return true if path points to a directory else returns false.
        /// <returns>FileInfo or DirectoryInfo object.</returns>
        /// The path is null.
        /// <exception cref="System.IO.IOException">
        /// I/O error occurs.
        /// <exception cref="System.UnauthorizedAccessException">
        /// An I/O error or a specific type of security error.
        private static FileSystemInfo GetFileSystemInfo(string path, out bool isContainer)
            FileSystemInfo fsinfo = new FileInfo(path);
            var attr = fsinfo.Attributes;
            var exists = (int)attr != -1;
            isContainer = exists && attr.HasFlag(FileAttributes.Directory);
            if (exists)
                    return new DirectoryInfo(path);
                    return fsinfo;
        /// Overrides the method of CmdletProvider, considering the additional
        /// dynamic parameters of FileSystemProvider.
        /// whether the filter or attribute filter is set.
        internal override bool IsFilterSet()
            bool attributeFilterSet = false;
            GetChildDynamicParameters fspDynamicParam = DynamicParameters as GetChildDynamicParameters;
            if (fspDynamicParam != null)
                attributeFilterSet = (
                    (fspDynamicParam.Attributes != null)
                        || (fspDynamicParam.Directory)
                        || (fspDynamicParam.File)
                        || (fspDynamicParam.Hidden)
                        || (fspDynamicParam.ReadOnly)
                        || (fspDynamicParam.System));
            return (attributeFilterSet || base.IsFilterSet());
        /// Gets the dynamic parameters for get-childnames on the
        /// We currently only support one dynamic parameter,
        /// "Attributes" that returns an enum evaluator for the
        /// given expression.
        protected override object GetChildNamesDynamicParameters(string path)
            return new GetChildDynamicParameters();
        /// Gets the dynamic parameters for get-childitems on the
        /// Gets the dynamic parameters for Copy-Item on the FileSystemProvider.
        /// <param name="path">Source for the copy operation.</param>
        /// <param name="destination">Destination for the copy operation.</param>
        /// <param name="recurse">Whether to recurse.</param>
        protected override object CopyItemDynamicParameters(string path, string destination, bool recurse)
            return new CopyItemDynamicParameters();
        #region ICmdletProviderSupportsHelp members
        /// Implementation of ICmdletProviderSupportsHelp interface.
        /// Gets provider-specific help content for the corresponding cmdlet.
        /// Name of command that the help is requested for.
        /// Not used here.
        /// The MAML help XML that should be presented to the user.
        public string GetHelpMaml(string helpItemName, string path)
            // Get the verb and noun from helpItemName
            XmlReader reader = null;
                // Load the help file from the current UI culture subfolder
                    string.IsNullOrEmpty(this.ProviderInfo.ApplicationBase) ? string.Empty : this.ProviderInfo.ApplicationBase,
                    string.IsNullOrEmpty(this.ProviderInfo.HelpFile) ? string.Empty : this.ProviderInfo.HelpFile);
                reader = XmlReader.Create(fullHelpPath, settings);
                    "[@id='FileSystem']",
                    ((IDisposable)reader).Dispose();
        #region CmdletProvider members
        /// Starts the File System provider. This method sets the Home for the
        /// provider to providerInfo.Home if specified, and %USERPROFILE%
        /// The ProviderInfo object that holds the provider's configuration.
        /// The updated ProviderInfo object that holds the provider's configuration.
        protected override ProviderInfo Start(ProviderInfo providerInfo)
            // Set the home folder for the user
            if (providerInfo != null && string.IsNullOrEmpty(providerInfo.Home))
                // %USERPROFILE% - indicate where a user's home directory is located in the file system.
                string homeDirectory = Environment.GetEnvironmentVariable(Platform.CommonEnvVariableNames.Home);
                if (!string.IsNullOrEmpty(homeDirectory))
                    if (Directory.Exists(homeDirectory))
                        s_tracer.WriteLine("Home = {0}", homeDirectory);
                        providerInfo.Home = homeDirectory;
                        s_tracer.WriteLine("Not setting home directory {0} - does not exist", homeDirectory);
            // OneDrive placeholder support (issue #8315)
            // make it so OneDrive placeholders are perceived as such with *all* their attributes accessible
            // The placeholder mode management APIs Rtl(Set|Query)(Process|Thread)PlaceholderCompatibilityMode
            // are only supported starting with Windows 10 version 1803 (build 17134)
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134, 0))
                // let's be safe, don't change the PlaceHolderCompatibilityMode if the current one is not what we expect
                if (Interop.Windows.RtlQueryProcessPlaceholderCompatibilityMode() == Interop.Windows.PHCM_DISGUISE_PLACEHOLDER)
                    Interop.Windows.RtlSetProcessPlaceholderCompatibilityMode(Interop.Windows.PHCM_EXPOSE_PLACEHOLDERS);
            return providerInfo;
        #endregion CmdletProvider members
        #region DriveCmdletProvider members
        /// Determines if the specified drive can be mounted.
        /// The drive that is going to be mounted.
        /// The same drive that was passed in, if the drive can be mounted.
        /// null if the drive cannot be mounted.
        /// drive is null.
        /// drive root is null or empty.
            // verify parameters
            if (string.IsNullOrEmpty(drive.Root))
                throw PSTraceSource.NewArgumentException("drive.Root");
            // -Persist switch parameter is supported only for Network paths.
            if (drive.Persist && !PathIsNetworkPath(drive.Root))
                ErrorRecord er = new ErrorRecord(new NotSupportedException(FileSystemProviderStrings.PersistNotSupported), "DriveRootNotNetworkPath", ErrorCategory.InvalidArgument, drive);
            if (IsNetworkMappedDrive(drive))
                // MapNetworkDrive facilitates to map the newly
                // created PS Drive to a network share.
                MapNetworkDrive(drive);
            // The drive is valid if the item exists or the
            // drive is not a fixed drive.  We want to allow
            // a drive to exist for floppies and other such\
            // removable media, even if the media isn't in place.
            bool driveIsFixed = true;
                // See if the drive is a fixed drive.
                string pathRoot = Path.GetPathRoot(drive.Root);
                DriveInfo driveInfo = new DriveInfo(pathRoot);
                if (driveInfo.DriveType != DriveType.Fixed)
                    driveIsFixed = false;
                // The current drive is a network drive.
                if (driveInfo.DriveType == DriveType.Network)
                    drive.IsNetworkDrive = true;
            catch (ArgumentException) // swallow ArgumentException incl. ArgumentNullException
            bool validDrive = true;
            if (driveIsFixed)
                // Since the drive is fixed, ensure the root is valid.
                validDrive = Directory.Exists(drive.Root);
            if (validDrive)
                result = drive;
                string error = StringUtil.Format(FileSystemProviderStrings.DriveRootError, drive.Root);
                Exception e = new IOException(error);
                WriteError(new ErrorRecord(e, "DriveRootError", ErrorCategory.ReadError, drive));
        /// MapNetworkDrive facilitates to map the newly created PS Drive to a network share.
        /// <param name="drive">The PSDrive info that would be used to create a new PS drive.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Can be static on Unix but not on Windows.")]
        private void MapNetworkDrive(PSDriveInfo drive)
            // Porting note: mapped network drives are only supported on Windows
            if (drive != null && !string.IsNullOrEmpty(drive.Root))
                // By default the connection is not persisted.
                int connectType = Interop.Windows.CONNECT_NOPERSIST;
                byte[] passwd = null;
                if (drive.Persist)
                    if (IsSupportedDriveForPersistence(drive))
                        connectType = Interop.Windows.CONNECT_UPDATE_PROFILE;
                        driveName = drive.Name + ":";
                        drive.DisplayRoot = drive.Root;
                        ErrorRecord er = new ErrorRecord(new InvalidOperationException(FileSystemProviderStrings.InvalidDriveName), "DriveNameNotSupportedForPersistence", ErrorCategory.InvalidOperation, drive);
                // If alternate credentials is supplied then use them to get connected to network share.
                if (drive.Credential != null && !drive.Credential.Equals(PSCredential.Empty))
                    userName = drive.Credential.UserName;
                    passwd = SecureStringHelper.GetData(drive.Credential.Password);
                    int errorCode = Interop.Windows.WNetAddConnection2(driveName, drive.Root, passwd, userName, connectType);
                    if (errorCode != Interop.Windows.ERROR_SUCCESS)
                        ErrorRecord er = new ErrorRecord(new System.ComponentModel.Win32Exception(errorCode), "CouldNotMapNetworkDrive", ErrorCategory.InvalidOperation, drive);
                    if (connectType == Interop.Windows.CONNECT_UPDATE_PROFILE)
                        // Update the current PSDrive to be a persisted drive.
                        // PsDrive.Root is updated to the name of the Drive for
                        // drives targeting network path and being persisted.
                        drive.Root = driveName + @"\";
                    // Clear the password in the memory.
                    if (passwd != null)
                        Array.Clear(passwd);
        /// ShouldMapNetworkDrive is a helper function used to detect if the
        /// requested PSDrive to be created has to be mapped to a network drive.
        private static bool IsNetworkMappedDrive(PSDriveInfo drive)
            bool shouldMapNetworkDrive = (drive != null && !string.IsNullOrEmpty(drive.Root) && PathIsNetworkPath(drive.Root)) &&
                                         (drive.Persist || (drive.Credential != null && !drive.Credential.Equals(PSCredential.Empty)));
            return shouldMapNetworkDrive;
        /// RemoveDrive facilitates to remove network mapped persisted PSDrvie.
        /// PSDrive info.
        /// <returns>PSDrive info.
                int flags = Interop.Windows.CONNECT_NOPERSIST;
                    // Here we are removing only persisted network drives.
                    flags = Interop.Windows.CONNECT_UPDATE_PROFILE;
                    // OSGTFS: 608188 PSDrive leaves a connection open after the drive is removed
                    // if a drive is not persisted or networkdrive, we need to use the actual root to remove the drive.
                    driveName = drive.Root;
                // You need to actually remove the drive.
                int errorCode = Interop.Windows.WNetCancelConnection2(driveName, flags, force: true);
                    ErrorRecord er = new ErrorRecord(new System.ComponentModel.Win32Exception(errorCode), "CouldRemoveNetworkDrive", ErrorCategory.InvalidOperation, drive);
        /// IsSupportedDriveForPersistence is a helper method used to
        /// check if the psdrive can be persisted or not.
        /// PS Drive Info.
        /// <returns>True if the drive can be persisted or else false.</returns>
        private static bool IsSupportedDriveForPersistence(PSDriveInfo drive)
            bool isSupportedDriveForPersistence = false;
                    isSupportedDriveForPersistence = true;
            return isSupportedDriveForPersistence;
        /// Return the UNC path for a given network drive
        /// using the Windows API.
        internal static string GetUNCForNetworkDrive(string driveName)
            return driveName;
            string uncPath = null;
            if (!string.IsNullOrEmpty(driveName) && driveName.Length == 1)
                int errorCode = Interop.Windows.GetUNCForNetworkDrive(driveName[0], out uncPath);
                    throw new System.ComponentModel.Win32Exception(errorCode);
            return uncPath;
        /// Get the substituted path of a NetWork type MS-DOS device that is created by 'subst' command.
        /// When a MS-DOS device is of NetWork type, it could be:
        ///   1. Substitute a path in a drive that maps to a network location. For example:
        ///         net use z: \\scratch2\scratch\
        ///         subst y: z:\abc\
        ///   2. Substitute a network location directly. For example:
        ///         subst y: \\scratch2\scratch\
        internal static string GetSubstitutedPathForNetworkDosDevice(string driveName)
            return WinGetSubstitutedPathForNetworkDosDevice(driveName);
        private static string WinGetSubstitutedPathForNetworkDosDevice(string driveName)
            string associatedPath = null;
                associatedPath = Interop.Windows.GetDosDeviceForNetworkPath(driveName[0]);
            return associatedPath;
        /// Get the root path for a network drive or MS-DOS device.
        /// <param name="driveInfo"></param>
        internal static string GetRootPathForNetworkDriveOrDosDevice(DriveInfo driveInfo)
            Dbg.Diagnostics.Assert(driveInfo.DriveType == DriveType.Network, "Caller should make sure it is a network drive.");
            string driveName = driveInfo.Name.Substring(0, 1);
            string rootPath = null;
                rootPath = GetUNCForNetworkDrive(driveName);
                if (driveInfo.IsReady)
                    // The drive is ready but we failed to find the UNC path based on the drive name.
                    // In this case, it's possibly a MS-DOS device created by 'subst' command that
                    //  - substitutes a network location directly, or
                    //  - substitutes a path in a drive that maps to a network location
                    rootPath = GetSubstitutedPathForNetworkDosDevice(driveName);
            return rootPath;
        /// Returns a collection of all logical drives in the system.
        /// A collection of PSDriveInfo objects, one for each logical drive returned from
        /// System.Environment.GetLogicalDrives().
            DriveInfo[] logicalDrives = DriveInfo.GetDrives();
            if (logicalDrives != null)
                foreach (DriveInfo newDrive in logicalDrives)
                    // cover everything by the try-catch block, because some of the
                    // DriveInfo properties may throw exceptions
                        string newDriveName = newDrive.Name.Substring(0, 1);
                        string root = newDrive.Name;
                        if (newDrive.DriveType == DriveType.Fixed)
                                description = newDrive.VolumeLabel;
                            // trying to read the volume label may cause an
                            // IOException or SecurityException. Just default
                            // to an empty description.
                        if (newDrive.DriveType == DriveType.Network)
                            // Platform notes: This is important because certain mount
                            // points on non-Windows are enumerated as drives by .NET, but
                            // the platform itself then has no real network drive support
                            // as required by this context. Solution: check for network
                            // drive support before using it.
                            displayRoot = GetRootPathForNetworkDriveOrDosDevice(newDrive);
                            if (!newDrive.RootDirectory.Exists)
                            root = newDrive.RootDirectory.FullName;
                        // Porting notes: On platforms with single root filesystems, ensure
                        // that we add a filesystem with the root "/" to the initial drive list,
                        // otherwise path handling will not work correctly because there
                        // is no : available to separate the filesystems from each other
                        if (root != StringLiterals.DefaultPathSeparatorString
                            && newDriveName == StringLiterals.DefaultPathSeparatorString)
                            root = StringLiterals.DefaultPathSeparatorString;
                        // Porting notes: On non-windows platforms .net can report two
                        // drives with the same root, make sure to only add one of those
                        bool skipDuplicate = false;
                        foreach (PSDriveInfo driveInfo in results)
                            if (driveInfo.Root == root)
                                skipDuplicate = true;
                        if (skipDuplicate)
                        // Create a new VirtualDrive for each logical drive
                                newDriveName,
                                root,
                        // The network drive is detected when PowerShell is launched.
                        // Hence it has been persisted during one of the earlier sessions,
                            newPSDriveInfo.IsNetworkDrive = true;
                        if (newDrive.DriveType != DriveType.Fixed)
                        // Porting notes: on the non-Windows platforms, the drive never
                        // uses : as a separator between drive and path
                            newPSDriveInfo.VolumeSeparatedByColon = false;
                        results.Add(newPSDriveInfo);
                    // If there are issues accessing properties of the DriveInfo, do
                    // not add the drive
                    DriveNames.TempDrive,
                    Path.GetTempPath(),
                    SessionStateStrings.TempDriveDescription,
                    credential: null,
                    displayRoot: null)
        #endregion DriveCmdletProvider methods
        #region ItemCmdletProvider methods
        /// Retrieves the dynamic parameters required for the Get-Item cmdlet.
        /// <param name="path">The path of the file to process.</param>
        /// <returns>An instance of the FileSystemProviderGetItemDynamicParameters class that represents the dynamic parameters.</returns>
            return new FileSystemProviderGetItemDynamicParameters();
        /// An example path looks like this
        ///     C:\WINNT\Media\chimes.wav.
        /// The fully qualified path to validate.
            // Path passed should be fully qualified path.
            // Remove alternate data stream references
            // See if they've used the inline stream syntax. They have more than one colon.
            int firstColon = path.IndexOf(':');
            int secondColon = path.IndexOf(':', firstColon + 1);
            if (secondColon > 0)
                path = path.Substring(0, secondColon);
            // Make sure the path is either drive rooted or UNC Path
            if (!IsAbsolutePath(path) && !Utils.PathIsUnc(path))
            // Exceptions should only deal with exceptional circumstances,
            // but unfortunately, FileInfo offers no Try() methods that
            // let us check if we _could_ open the file.
                FileInfo testFile = new FileInfo(path);
                if ((e is ArgumentNullException) ||
                    (e is PathTooLongException) ||
                    (e is NotSupportedException))
            // .NET introduced a change where invalid characters are accepted https://learn.microsoft.com/en-us/dotnet/core/compatibility/2.1#path-apis-dont-throw-an-exception-for-invalid-characters
            // We need to check for invalid characters ourselves.  `Path.GetInvalidFileNameChars()` is a supserset of `Path.GetInvalidPathChars()`
            // Remove drive root first
            string pathWithoutDriveRoot = path.Substring(Path.GetPathRoot(path).Length);
            foreach (string segment in pathWithoutDriveRoot.Split(Path.DirectorySeparatorChar))
                if (PathUtils.ContainsInvalidFileNameChars(segment))
        /// A fully qualified path representing a file or directory in the
        /// file system.
        /// Nothing.  FileInfo and DirectoryInfo objects are written to the
        /// context's pipeline.
            // Validate the argument
                // The parameter was null, throw an exception
                bool retrieveStreams = false;
                FileSystemProviderGetItemDynamicParameters dynamicParameters = null;
                if (DynamicParameters != null)
                    dynamicParameters = DynamicParameters as FileSystemProviderGetItemDynamicParameters;
                    if (dynamicParameters != null)
                        if ((dynamicParameters.Stream != null) && (dynamicParameters.Stream.Length > 0))
                            retrieveStreams = true;
                                string streamName = path.Substring(secondColon + 1);
                                path = path.Remove(secondColon);
                                dynamicParameters = new FileSystemProviderGetItemDynamicParameters();
                                dynamicParameters.Stream = new string[] { streamName };
                FileSystemInfo result = GetFileSystemItem(path, ref isContainer, false);
                    // If we want to retrieve the file streams, retrieve them.
                    if (retrieveStreams)
                        foreach (string desiredStream in dynamicParameters.Stream)
                            // See that it matches the name specified
                            WildcardPattern p = WildcardPattern.Get(desiredStream, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                            bool foundStream = false;
                            foreach (AlternateStreamData stream in AlternateDataStreamUtilities.GetStreams(result.FullName))
                                if (!p.IsMatch(stream.Stream))
                                string outputPath = result.FullName + ":" + stream.Stream;
                                // Alternate data streams can never be containers.
                                WriteItemObject(stream, outputPath, isContainer: false);
                                foundStream = true;
                            if ((!WildcardPattern.ContainsWildcardCharacters(desiredStream)) && (!foundStream))
                                    FileSystemProviderStrings.AlternateDataStreamNotFound, desiredStream, result.FullName);
                                Exception e = new FileNotFoundException(errorMessage, result.FullName);
                                    "AlternateDataStreamNotFound",
                        // Otherwise, return the item itself.
                        WriteItemObject(result, result.FullName, isContainer);
                    string error = StringUtil.Format(FileSystemProviderStrings.ItemNotFound, path);
            catch (IOException ioError)
                // IOException contains specific message about the error occurred and so no need for errordetails.
                ErrorRecord er = new ErrorRecord(ioError, "GetItemIOError", ErrorCategory.ReadError, path);
                WriteError(new ErrorRecord(accessException, "GetItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        private FileSystemInfo GetFileSystemItem(string path, ref bool isContainer, bool showHidden)
            FileInfo result = new FileInfo(path);
            var attributes = result.Attributes;
            bool hidden = attributes.HasFlag(FileAttributes.Hidden);
            isContainer = attributes.HasFlag(FileAttributes.Directory);
            // FileInfo allows for a file path to end in a trailing slash, but the resulting object
            // is incomplete.  A trailing slash should indicate a directory.  So if the path ends in a
            // trailing slash and is not a directory, return null
            if (!isContainer && path.EndsWith(Path.DirectorySeparatorChar))
            FlagsExpression<FileAttributes> evaluator = null;
            FlagsExpression<FileAttributes> switchEvaluator = null;
                evaluator = fspDynamicParam.Attributes;
                switchEvaluator = FormatAttributeSwitchParameters();
            bool filterHidden = false;           // "Hidden" is specified somewhere in the expression
            bool switchFilterHidden = false;     // "Hidden" is specified somewhere in the parameters
            if (evaluator != null)
                filterHidden = evaluator.ExistsInExpression(FileAttributes.Hidden);
            if (switchEvaluator != null)
                switchFilterHidden = switchEvaluator.ExistsInExpression(FileAttributes.Hidden);
            // if "Hidden" is specified in the attribute filter dynamic parameters
            // also return the object
            if (!isContainer)
                if (!hidden || Force || showHidden || filterHidden || switchFilterHidden)
                    s_tracer.WriteLine("Got file info: {0}", result);
                // Check to see if the path is the root of a file system drive.
                // Since all root paths are hidden we need to show the directory
                // anyway
                bool isRootPath =
                        Path.GetPathRoot(path),
                        result.FullName,
                if (isRootPath || !hidden || Force || showHidden || filterHidden || switchFilterHidden)
                    s_tracer.WriteLine("Got directory info: {0}", result);
        /// Invokes the item at the path using ShellExecute semantics.
        /// The item to invoke.
            string action = FileSystemProviderStrings.InvokeItemAction;
            string resource = StringUtil.Format(FileSystemProviderStrings.InvokeItemResourceFileTemplate, path);
                var invokeProcess = new System.Diagnostics.Process();
                // codeql[cs/microsoft/command-line-injection-shell-execution] - This is expected Poweshell behavior where user inputted paths are supported for the context of this method. The user assumes trust for the file path they are specifying. If there is concern for remoting, restricted remoting guidelines should be used.
                invokeProcess.StartInfo.FileName = path;
                bool useShellExecute = false;
                    // Path points to a directory. We have to use xdg-open/open on Linux/macOS.
                    useShellExecute = true;
                        // Try Process.Start first. This works for executables on Win/Unix platforms
                        invokeProcess.Start();
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 13)
                        // Error code 13 -- Permission denied
                        // The file is possibly not an executable. We try xdg-open/open on Linux/macOS.
                if (useShellExecute)
                    invokeProcess.StartInfo.UseShellExecute = true;
                // Use ShellExecute when it's not a headless SKU
                invokeProcess.StartInfo.UseShellExecute = Platform.IsWindowsDesktop;
        #endregion ItemCmdletProvider members
        #region ContainerCmdletProvider members
        /// Gets the child items of a given directory.
        /// The full path of the directory to enumerate.
        /// Nothing.  FileInfo and DirectoryInfo objects that match the filter are written to the
        protected override void GetChildItems(
            GetPathItems(path, recurse, depth, false, ReturnContainers.ReturnMatchingContainers);
        /// Gets the path names for all children of the specified
        /// directory that match the given filter.
        /// Nothing.  Child names are written to the context's pipeline.
            GetPathItems(path, false, uint.MaxValue, true, returnContainers);
        /// Makes no attempt to filter if the user has already specified a filter, or
        /// if the path contains directory separators. Those are not supported by the
        /// FileSystem filter.
        protected override bool ConvertPath(
            // Don't handle full paths, paths that the user is already trying to
            // filter, or paths they are trying to escape.
            if ((!string.IsNullOrEmpty(filter)) ||
                (path.Contains(StringLiterals.DefaultPathSeparator, StringComparison.Ordinal)) ||
                (path.Contains(StringLiterals.AlternatePathSeparator, StringComparison.Ordinal)) ||
                (path.Contains(StringLiterals.EscapeCharacter)))
            // We can never actually modify the PowerShell path, as the
            // Win32 filtering support returns items that match the short
            // filename OR long filename.
            // This creates tons of seemingly incorrect matches, such as:
            // *~*:   Matches any file with a long filename
            // *n*:   Matches all files with a long filename, but have been
            //        mapped to a [6][~n].[3] disambiguation bucket
            // *.abc: Matches all files that have an extension that begins
            //        with ABC, since their extension is truncated in the
            //        short filename
            // *.*:   Matches all files and directories, even if they don't
            //        have a dot in their name
            // Our algorithm here is pretty simple. The filesystem can handle
            // * and ? in PowerShell wildcards, just not character ranges [a-z].
            // We replace character ranges with the single-character wildcard, '?'.
            updatedPath = path;
            updatedFilter = System.Text.RegularExpressions.Regex.Replace(path, "\\[.*?\\]", "?");
        private void GetPathItems(
            bool nameOnly,
            var fsinfo = GetFileSystemInfo(path, out bool isDirectory);
            if (fsinfo != null)
                if (isDirectory)
                    InodeTracker tracker = null;
                        if (fspDynamicParam != null && fspDynamicParam.FollowSymlink)
                            tracker = new InodeTracker(fsinfo.FullName);
                    // Enumerate the directory
                    Dir((DirectoryInfo)fsinfo, recurse, depth, nameOnly, returnContainers, tracker);
                    bool attributeFilter = true;
                    bool switchAttributeFilter = true;
                        attributeFilter = evaluator.Evaluate(fsinfo.Attributes);  // expressions
                        switchAttributeFilter = switchEvaluator.Evaluate(fsinfo.Attributes);  // switch parameters
                    bool hidden = (fsinfo.Attributes & FileAttributes.Hidden) != 0;
                    // if "Hidden" is explicitly specified anywhere in the attribute filter, then override
                    // default hidden attribute filter.
                    if ((attributeFilter && switchAttributeFilter)
                        && (filterHidden || switchFilterHidden || Force || !hidden))
                        if (nameOnly)
                                fsinfo.Name,
                                fsinfo.FullName,
                            WriteItemObject(fsinfo, path, false);
                string error = StringUtil.Format(FileSystemProviderStrings.ItemDoesNotExist, path);
                    "ItemDoesNotExist",
        private void Dir(
            DirectoryInfo directory,
            InodeTracker tracker)   // tracker will be non-null only if the user invoked the -FollowSymLinks and -Recurse switch parameters.
            List<IEnumerable<FileSystemInfo>> target = new List<IEnumerable<FileSystemInfo>>();
                if (Filter != null &&
                    Filter.Length > 0)
                    if (returnContainers == ReturnContainers.ReturnAllContainers)
                        // Don't filter directories
                        target.Add(directory.EnumerateDirectories());
                        // Filter the directories
                        target.Add(directory.EnumerateDirectories(Filter, _enumerationOptions));
                    // Use the specified filter when retrieving the
                    // children
                    target.Add(directory.EnumerateFiles(Filter, _enumerationOptions));
                    // Don't use a filter to retrieve the children
                    target.Add(directory.EnumerateFiles());
                // Write out the items
                foreach (IEnumerable<FileSystemInfo> childList in target)
                    // On some systems, this is already sorted.  For consistency, always sort again.
                    IEnumerable<FileSystemInfo> sortedChildList = childList.OrderBy(static c => c.Name, StringComparer.CurrentCultureIgnoreCase);
                    foreach (FileSystemInfo filesystemInfo in sortedChildList)
                            // 'Hidden' is specified somewhere in the expression
                            bool filterHidden = false;
                            // 'Hidden' is specified somewhere in the parameters
                            bool switchFilterHidden = false;
                                attributeFilter = evaluator.Evaluate(filesystemInfo.Attributes);
                                switchAttributeFilter = switchEvaluator.Evaluate(filesystemInfo.Attributes);
                            bool hidden = false;
                                hidden = (filesystemInfo.Attributes & FileAttributes.Hidden) != 0;
                            // If 'Hidden' is explicitly specified anywhere in the attribute filter, then override
                            // If specification is to return all containers, then do not do attribute filter on
                            // the containers.
                            bool attributeSatisfy =
                                ((attributeFilter && switchAttributeFilter) ||
                                    ((returnContainers == ReturnContainers.ReturnAllContainers) &&
                                    ((filesystemInfo.Attributes & FileAttributes.Directory) != 0)));
                            if (attributeSatisfy && (filterHidden || switchFilterHidden || Force || !hidden))
                                        filesystemInfo.Name,
                                        filesystemInfo.FullName,
                                    if (filesystemInfo is FileInfo)
                                        WriteItemObject(filesystemInfo, filesystemInfo.FullName, false);
                                        WriteItemObject(filesystemInfo, filesystemInfo.FullName, true);
                        catch (System.IO.FileNotFoundException ex)
                            WriteError(new ErrorRecord(ex, "DirIOError", ErrorCategory.ReadError, directory.FullName));
                            WriteError(new ErrorRecord(ex, "DirUnauthorizedAccessError", ErrorCategory.PermissionDenied, directory.FullName));
                bool isFilterHiddenSpecified = false;           // "Hidden" is specified somewhere in the expression
                bool isSwitchFilterHiddenSpecified = false;     // "Hidden" is specified somewhere in the parameters
                    isFilterHiddenSpecified = evaluator.ExistsInExpression(FileAttributes.Hidden);
                    isSwitchFilterHiddenSpecified = switchEvaluator.ExistsInExpression(FileAttributes.Hidden);
                // Recurse into the directories
                // Grab all the directories to recurse
                // into separately from the ones that will get written
                // out.
                        foreach (DirectoryInfo recursiveDirectory in directory.EnumerateDirectories())
                            bool checkReparsePoint = true;
                                hidden = (recursiveDirectory.Attributes & FileAttributes.Hidden) != 0;
                                // We've already taken the expense of initializing the Attributes property here,
                                // so we can use that to avoid needing to call IsReparsePointLikeSymlink() later.
                                checkReparsePoint = recursiveDirectory.Attributes.HasFlag(FileAttributes.ReparsePoint);
                            if (Force || !hidden || isFilterHiddenSpecified || isSwitchFilterHiddenSpecified)
                                // We only want to recurse into symlinks if
                                //  a) the user has asked to with the -FollowSymLinks switch parameter and
                                //  b) the directory pointed to by the symlink has not already been visited,
                                //     preventing symlink loops.
                                //  c) it is not a reparse point with a target (not OneDrive or an AppX link).
                                if (tracker == null)
                                    if (checkReparsePoint && InternalSymbolicLinkLinkCodeMethods.IsReparsePointLikeSymlink(recursiveDirectory))
                                else if (!tracker.TryVisitPath(recursiveDirectory.FullName))
                                    WriteWarning(StringUtil.Format(FileSystemProviderStrings.AlreadyListedDirectory,
                                                                   recursiveDirectory.FullName));
                                Dir(recursiveDirectory, recurse, depth - 1, nameOnly, returnContainers, tracker);
                WriteError(new ErrorRecord(argException, "DirArgumentError", ErrorCategory.InvalidArgument, directory.FullName));
                // 2004/10/13-JonN removed ResourceActionFailedException wrapper
                WriteError(new ErrorRecord(e, "DirIOError", ErrorCategory.ReadError, directory.FullName));
                WriteError(new ErrorRecord(uae, "DirUnauthorizedAccessError", ErrorCategory.PermissionDenied, directory.FullName));
        /// Create an enum expression evaluator for user-specified attribute filtering
        /// switch parameters.
        /// If any attribute filtering switch parameters are set,
        /// returns an evaluator that evaluates these parameters.
        /// Otherwise,
        /// returns NULL
        private FlagsExpression<FileAttributes> FormatAttributeSwitchParameters()
            FlagsExpression<FileAttributes> switchParamEvaluator = null;
            if (((GetChildDynamicParameters)DynamicParameters).Directory)
                sb.Append("+Directory");
            if (((GetChildDynamicParameters)DynamicParameters).File)
                sb.Append("+!Directory");
            if (((GetChildDynamicParameters)DynamicParameters).System)
                sb.Append("+System");
            if (((GetChildDynamicParameters)DynamicParameters).ReadOnly)
                sb.Append("+ReadOnly");
            if (((GetChildDynamicParameters)DynamicParameters).Hidden)
                sb.Append("+Hidden");
            string switchParamString = sb.ToString();
            if (!string.IsNullOrEmpty(switchParamString))
                // Remove unnecessary PLUS sign
                switchParamEvaluator = new FlagsExpression<FileAttributes>(switchParamString.Substring(1));
            return switchParamEvaluator;
        /// Provides a mode property for FileSystemInfo.
        /// <param name="instance">Instance of PSObject wrapping a FileSystemInfo.</param>
        /// <returns>A string representation of the FileAttributes, with one letter per attribute.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        public static string Mode(PSObject instance) => Mode(instance, excludeHardLink: false);
        /// Provides a ModeWithoutHardLink property for FileSystemInfo, without HardLinks for performance reasons.
        public static string ModeWithoutHardLink(PSObject instance) => Mode(instance, excludeHardLink: true);
        private static string Mode(PSObject instance, bool excludeHardLink)
            string ToModeString(FileSystemInfo fileSystemInfo)
                FileAttributes fileAttributes = fileSystemInfo.Attributes;
                bool isReparsePoint = InternalSymbolicLinkLinkCodeMethods.IsReparsePoint(fileSystemInfo);
                bool isLink = isReparsePoint || (!excludeHardLink && InternalSymbolicLinkLinkCodeMethods.IsHardLink(fileSystemInfo));
                if (!isLink)
                    // special casing for the common cases - no allocations
                    switch (fileAttributes)
                        case FileAttributes.Archive:
                            return "-a---";
                        case FileAttributes.Directory:
                            return "d----";
                        case FileAttributes.Normal:
                            return "-----";
                        case FileAttributes.Directory | FileAttributes.ReadOnly:
                            return "d-r--";
                        case FileAttributes.Archive | FileAttributes.ReadOnly:
                            return "-ar--";
                ReadOnlySpan<char> mode =
                [
                    isLink ?
                        'l' :
                        fileAttributes.HasFlag(FileAttributes.Directory) ? 'd' : '-',
                    fileAttributes.HasFlag(FileAttributes.Archive) ? 'a' : '-',
                    fileAttributes.HasFlag(FileAttributes.ReadOnly) ? 'r' : '-',
                    fileAttributes.HasFlag(FileAttributes.Hidden) ? 'h' : '-',
                    fileAttributes.HasFlag(FileAttributes.System) ? 's' : '-',
                return new string(mode);
            return instance?.BaseObject is FileSystemInfo fileInfo
                ? ToModeString(fileInfo)
        /// Provides a NameString property for FileSystemInfo.
        /// <returns>Name if a file or directory, Name -> Target if symlink.</returns>
        public static string NameString(PSObject instance)
            if (instance?.BaseObject is FileSystemInfo fileInfo)
                if (InternalSymbolicLinkLinkCodeMethods.IsReparsePointLikeSymlink(fileInfo))
                    return $"{PSStyle.Instance.FileInfo.SymbolicLink}{fileInfo.Name}{PSStyle.Instance.Reset} -> {fileInfo.LinkTarget}";
                else if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
                    return $"{PSStyle.Instance.FileInfo.Directory}{fileInfo.Name}{PSStyle.Instance.Reset}";
                else if (PSStyle.Instance.FileInfo.Extension.ContainsKey(fileInfo.Extension))
                    return $"{PSStyle.Instance.FileInfo.Extension[fileInfo.Extension]}{fileInfo.Name}{PSStyle.Instance.Reset}";
                else if ((Platform.IsWindows && CommandDiscovery.PathExtensions.Contains(fileInfo.Extension.ToLower())) ||
                    (!Platform.IsWindows && Platform.NonWindowsIsExecutable(fileInfo.FullName)))
                    return $"{PSStyle.Instance.FileInfo.Executable}{fileInfo.Name}{PSStyle.Instance.Reset}";
                    return fileInfo.Name;
        /// Provides a LengthString property for FileSystemInfo.
        /// <returns>Length as a string.</returns>
        public static string LengthString(PSObject instance)
            return instance?.BaseObject is FileInfo fileInfo
                ? fileInfo.Attributes.HasFlag(FileAttributes.Offline)
                    ? $"({fileInfo.Length})"
                    : fileInfo.Length.ToString()
        /// Provides a LastWriteTimeString property for FileSystemInfo.
        /// <returns>LastWriteTime formatted as short date + short time.</returns>
        public static string LastWriteTimeString(PSObject instance)
                ? string.Create(CultureInfo.CurrentCulture, $"{fileInfo.LastWriteTime,10:d} {fileInfo.LastWriteTime,8:t}")
        /// Renames a file or directory.
        /// The current full path to the file or directory.
        /// The new full path to the file or directory.
        /// Nothing.  The renamed DirectoryInfo or FileInfo object is
        ///     newName is null or empty
        protected override void RenameItem(
            // Check the parameters
            // Clean up "newname" to fix some common usability problems:
            // Rename .\foo.txt .\bar.txt
            // Rename C:\temp\foo.txt C:\temp\bar.txt
            if (newName.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) ||
                newName.StartsWith("./", StringComparison.OrdinalIgnoreCase))
                newName = newName.Remove(0, 2);
            else if (string.Equals(Path.GetDirectoryName(path), Path.GetDirectoryName(newName), StringComparison.OrdinalIgnoreCase))
                newName = Path.GetFileName(newName);
            // Check to see if the target specified is just filename. We dont allow rename to move the file to a different directory.
            // If a path is specified for the newName then we flag that as an error.
            if (!string.Equals(Path.GetFileName(newName), newName, StringComparison.OrdinalIgnoreCase))
                throw PSTraceSource.NewArgumentException(nameof(newName), FileSystemProviderStrings.RenameError);
            // Verify that the target doesn't represent a device name
            if (PathIsReservedDeviceName(newName, "RenameError"))
                bool isContainer = IsItemContainer(path);
                FileSystemInfo result = null;
                    // Get the DirectoryInfo
                    DirectoryInfo dir = new DirectoryInfo(path);
                    // Generate the new path which the directory will
                    // be renamed to.
                    string parentDirectory = dir.Parent.FullName;
                    string newPath = MakePath(parentDirectory, newName);
                    // Confirm the rename with the user
                    string action = FileSystemProviderStrings.RenameItemActionDirectory;
                    string resource = StringUtil.Format(FileSystemProviderStrings.RenameItemResourceFileTemplate, dir.FullName, newPath);
                        // Now move the file
                        dir.MoveTo(newPath);
                        result = dir;
                    // Get the FileInfo
                    FileInfo file = new FileInfo(path);
                    // Generate the new path which the file will be renamed to.
                    string parentDirectory = file.DirectoryName;
                    string action = FileSystemProviderStrings.RenameItemActionFile;
                    string resource = StringUtil.Format(FileSystemProviderStrings.RenameItemResourceFileTemplate, file.FullName, newPath);
                        file.MoveTo(newPath);
                        result = file;
                WriteError(new ErrorRecord(argException, "RenameItemArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "RenameItemIOError", ErrorCategory.WriteError, path));
                WriteError(new ErrorRecord(accessException, "RenameItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        /// Creates a file or directory with the given path.
        /// The path of the file or directory to create.
        /// Specify "file" to create a file.
        /// Specify "directory" or "container" to create a directory.
        /// If <paramref name="type"/> is "file" then this parameter becomes the content
        /// of the file to be created.
        /// Nothing.  The new DirectoryInfo or FileInfo object is
        ///     type is null or empty.
            ItemType itemType = ItemType.Unknown;
                type = "file";
                if (!CreateIntermediateDirectories(path))
            itemType = GetItemType(type);
            if (itemType == ItemType.Directory)
                CreateDirectory(path, true);
            else if (itemType == ItemType.File)
                    FileMode fileMode = FileMode.CreateNew;
                        // If force is specified, overwrite the existing
                        // file
                        fileMode = FileMode.Create;
                    string action = FileSystemProviderStrings.NewItemActionFile;
                    string resource = StringUtil.Format(FileSystemProviderStrings.NewItemActionTemplate, path);
                        // Create the file with read/write access and
                        // not allowing sharing.
                        using (FileStream newFile =
                            new FileStream(
                                FileShare.None))
                                StreamWriter streamWriter = new StreamWriter(newFile);
                                streamWriter.Write(value.ToString());
                                streamWriter.Flush();
                        FileInfo fileInfo = new FileInfo(path);
                        WriteItemObject(fileInfo, path, false);
                catch (IOException exception)
                    WriteError(new ErrorRecord(exception, "NewItemIOError", ErrorCategory.WriteError, path));
                    WriteError(new ErrorRecord(accessException, "NewItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            else if (itemType == ItemType.SymbolicLink || itemType == ItemType.HardLink)
                if (itemType == ItemType.SymbolicLink)
                    action = FileSystemProviderStrings.NewItemActionSymbolicLink;
                else if (itemType == ItemType.HardLink)
                    action = FileSystemProviderStrings.NewItemActionHardLink;
                    bool isDirectory = false;
                    string strTargetPath = value?.ToString();
                    if (string.IsNullOrEmpty(strTargetPath))
                    bool exists = false;
                    // It is legal to create symbolic links to non-existing targets on
                    // both Windows and Linux. It is not legal to create hard links to
                    // non-existing targets on either Windows or Linux.
                            exists = true;
                            // unify directory separators to be consistent with the rest of PowerShell even on non-Windows platforms;
                            // do this before resolving the target, otherwise e.g. `.\test` would break on Linux, since the combined
                            // path below would be something like `/path/to/cwd/.\test`
                            strTargetPath = strTargetPath.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
                            // check if the target is a file or directory
                            var normalizedTargetPath = Path.Combine(Path.GetDirectoryName(path), strTargetPath);
                            GetFileSystemInfo(normalizedTargetPath, out isDirectory);
                            // for hardlinks we resolve the target to an absolute path
                            if (!IsAbsolutePath(strTargetPath))
                                strTargetPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(strTargetPath);
                            exists = GetFileSystemInfo(strTargetPath, out isDirectory) != null;
                        WriteError(new ErrorRecord(e, "AccessException", ErrorCategory.PermissionDenied, strTargetPath));
                        string message = StringUtil.Format(FileSystemProviderStrings.ItemNotFound, strTargetPath);
                        WriteError(new ErrorRecord(new ItemNotFoundException(message), "ItemNotFound", ErrorCategory.ObjectNotFound, strTargetPath));
                    if (itemType == ItemType.HardLink)
                        // Hard links can only be to files, not directories.
                            string message = StringUtil.Format(FileSystemProviderStrings.ItemNotFile, strTargetPath);
                            WriteError(new ErrorRecord(new InvalidOperationException(message), "ItemNotFile", ErrorCategory.InvalidOperation, strTargetPath));
                    bool isSymLinkDirectory = false;
                    bool symLinkExists = false;
                        symLinkExists = GetFileSystemInfo(path, out isSymLinkDirectory) != null;
                        WriteError(new ErrorRecord(e, "AccessException", ErrorCategory.PermissionDenied, path));
                        if (itemType == ItemType.HardLink && string.Equals(path, strTargetPath, StringComparison.OrdinalIgnoreCase))
                            string message = StringUtil.Format(FileSystemProviderStrings.NewItemTargetIsSameAsLink, path);
                            WriteError(new ErrorRecord(new InvalidOperationException(message), "TargetIsSameAsLink", ErrorCategory.InvalidOperation, path));
                            if (!isSymLinkDirectory && symLinkExists)
                                File.Delete(path);
                            else if (isSymLinkDirectory && symLinkExists)
                                Directory.Delete(path);
                            if ((exception is FileNotFoundException) ||
                                (exception is DirectoryNotFoundException) ||
                                (exception is UnauthorizedAccessException) ||
                                (exception is System.Security.SecurityException) ||
                                (exception is ArgumentException) ||
                                (exception is PathTooLongException) ||
                                (exception is NotSupportedException) ||
                                (exception is ArgumentNullException) ||
                                (exception is IOException))
                                WriteError(new ErrorRecord(exception, "NewItemDeleteIOError", ErrorCategory.WriteError, path));
                        if (symLinkExists)
                            string message = StringUtil.Format(FileSystemProviderStrings.SymlinkItemExists, path);
                            WriteError(new ErrorRecord(new IOException(message), "SymLinkExists", ErrorCategory.ResourceExists, path));
                        success = Platform.NonWindowsCreateSymbolicLink(path, strTargetPath);
                        success = WinCreateSymbolicLink(path, strTargetPath, isDirectory);
                        success = Platform.NonWindowsCreateHardLink(path, strTargetPath);
                        success = Interop.Windows.CreateHardLink(path, strTargetPath, IntPtr.Zero);
                        // Porting note: The Win32Exception will report the correct error on Linux
                        Win32Exception w32Exception = new Win32Exception((int)errorCode);
                        if (Platform.Unix.GetErrorCategory(errorCode) == ErrorCategory.PermissionDenied)
                        if (errorCode == 1314) // ERROR_PRIVILEGE_NOT_HELD
                            string message = FileSystemProviderStrings.ElevationRequired;
                            WriteError(new ErrorRecord(new UnauthorizedAccessException(message, w32Exception), "NewItemSymbolicLinkElevationRequired", ErrorCategory.PermissionDenied, value.ToString()));
                        if (errorCode == 1) // ERROR_INVALID_FUNCTION
                                message = FileSystemProviderStrings.SymbolicLinkNotSupported;
                                message = FileSystemProviderStrings.HardLinkNotSupported;
                            WriteError(new ErrorRecord(new InvalidOperationException(message, w32Exception), "NewItemInvalidOperation", ErrorCategory.InvalidOperation, value.ToString()));
                        throw w32Exception;
                            DirectoryInfo dirInfo = new DirectoryInfo(path);
                            WriteItemObject(dirInfo, path, true);
            else if (itemType == ItemType.Junction)
                string action = FileSystemProviderStrings.NewItemActionJunction;
                   // junctions require an absolute path
                    if (!Path.IsPathRooted(strTargetPath))
                        WriteError(new ErrorRecord(new ArgumentException(FileSystemProviderStrings.JunctionAbsolutePath), "NotAbsolutePath", ErrorCategory.InvalidArgument, strTargetPath));
                    // Junctions can only be directories.
                    if (!isDirectory)
                        string message = StringUtil.Format(FileSystemProviderStrings.ItemNotDirectory, value);
                        WriteError(new ErrorRecord(new InvalidOperationException(message), "ItemNotDirectory", ErrorCategory.InvalidOperation, value));
                    bool isPathDirectory = false;
                    FileSystemInfo pathDirInfo;
                        pathDirInfo = GetFileSystemInfo(path, out isPathDirectory);
                    bool pathExists = pathDirInfo != null;
                    if (pathExists)
                        if (!isPathDirectory)
                            string message = StringUtil.Format(FileSystemProviderStrings.ItemNotDirectory, path);
                            WriteError(new ErrorRecord(new InvalidOperationException(message), "ItemNotDirectory", ErrorCategory.InvalidOperation, path));
                        // Junctions cannot have files
                        if (!Force && DirectoryInfoHasChildItems((DirectoryInfo)pathDirInfo))
                            string message = StringUtil.Format(FileSystemProviderStrings.DirectoryNotEmpty, path);
                            WriteError(new ErrorRecord(new IOException(message), "DirectoryNotEmpty", ErrorCategory.WriteError, path));
                            pathDirInfo.Delete();
                            if ((exception is DirectoryNotFoundException) ||
                    CreateDirectory(path, streamOutput: false);
                    pathDirInfo = new DirectoryInfo(path);
                        bool junctionCreated = WinCreateJunction(path, strTargetPath);
                        if (junctionCreated)
                            WriteItemObject(pathDirInfo, path, true);
                        else // rollback the directory creation if we created it.
                            if (!pathExists)
                        // rollback the directory creation if it was created.
                                (exception is Win32Exception) ||
                            WriteError(new ErrorRecord(exception, "NewItemCreateIOError", ErrorCategory.WriteError, path));
                throw PSTraceSource.NewArgumentException(nameof(type), FileSystemProviderStrings.UnknownType);
        private static bool WinCreateSymbolicLink(string path, string strTargetPath, bool isDirectory)
            // The new AllowUnprivilegedCreate is only available on Win10 build 14972 or newer
            var flags = isDirectory ? Interop.Windows.SymbolicLinkFlags.Directory : Interop.Windows.SymbolicLinkFlags.File;
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14972, 0))
                flags |= Interop.Windows.SymbolicLinkFlags.AllowUnprivilegedCreate;
            var created = Interop.Windows.CreateSymbolicLink(path, strTargetPath, flags);
            return created;
        private static bool WinCreateJunction(string path, string strTargetPath)
            bool junctionCreated = InternalSymbolicLinkLinkCodeMethods.CreateJunction(path, strTargetPath);
            return junctionCreated;
        private enum ItemType
            Junction,
            HardLink
        private static ItemType GetItemType(string input)
            WildcardPattern typeEvaluator =
                WildcardPattern.Get(input + "*",
                                     WildcardOptions.IgnoreCase |
                                     WildcardOptions.Compiled);
            if (typeEvaluator.IsMatch("directory") ||
                typeEvaluator.IsMatch("container"))
                itemType = ItemType.Directory;
            else if (typeEvaluator.IsMatch("file"))
                itemType = ItemType.File;
            else if (typeEvaluator.IsMatch("symboliclink"))
                itemType = ItemType.SymbolicLink;
            else if (typeEvaluator.IsMatch("junction"))
                itemType = ItemType.Junction;
            else if (typeEvaluator.IsMatch("hardlink"))
                itemType = ItemType.HardLink;
            return itemType;
        /// Creates a directory at the specified path.
        /// The path of the directory to create
        /// <param name="streamOutput">
        /// Determines if the directory should be streamed out after being created.
        private void CreateDirectory(string path, bool streamOutput)
                !string.IsNullOrEmpty(path),
                "The caller should verify path");
            if (!Force && ItemExists(path, out error))
                string errorMessage = StringUtil.Format(FileSystemProviderStrings.DirectoryExist, path);
                Exception e = new IOException(errorMessage);
                    "DirectoryExist",
                string action = FileSystemProviderStrings.NewItemActionDirectory;
                    var result = Directory.CreateDirectory(path);
                    if (streamOutput)
                        // Write the result to the pipeline
                        WriteItemObject(result, path, true);
                WriteError(new ErrorRecord(argException, "CreateDirectoryArgumentError", ErrorCategory.InvalidArgument, path));
                // Windows error code for invalid characters in file or directory name
                const int ERROR_INVALID_NAME = unchecked((int)0x8007007B);
                // Do not suppress IOException on Windows if it has the specific HResult for invalid characters in directory name
                if (ioException.HResult == ERROR_INVALID_NAME || !Force)
                    WriteError(new ErrorRecord(ioException, "CreateDirectoryIOError", ErrorCategory.WriteError, path));
                WriteError(new ErrorRecord(accessException, "CreateDirectoryUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        private bool CreateIntermediateDirectories(string path)
                // Push the paths of the missing directories onto a stack such that the highest missing
                // parent in the tree is at the top of the stack.
                Stack<string> missingDirectories = new Stack<string>();
                string previousParent = path;
                    string root = string.Empty;
                    if (PSDriveInfo != null)
                        root = PSDriveInfo.Root;
                    string parentPath = GetParentPath(path, root);
                    if (!string.IsNullOrEmpty(parentPath) &&
                            parentPath,
                            previousParent,
                        if (!ItemExists(parentPath))
                            missingDirectories.Push(parentPath);
                    previousParent = parentPath;
                } while (!string.IsNullOrEmpty(previousParent));
                // Now create the missing directories
                foreach (string directoryPath in missingDirectories)
                    CreateDirectory(directoryPath, false);
                WriteError(new ErrorRecord(argException, "CreateIntermediateDirectoriesArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "CreateIntermediateDirectoriesIOError", ErrorCategory.WriteError, path));
                WriteError(new ErrorRecord(accessException, "CreateIntermediateDirectoriesUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        /// Removes the specified file or directory.
        /// The full path to the file or directory to be removed.
        /// Specifies if the operation should also remove child items.
                bool removeStreams = false;
                FileSystemProviderRemoveItemDynamicParameters dynamicParameters = null;
                    dynamicParameters = DynamicParameters as FileSystemProviderRemoveItemDynamicParameters;
                            removeStreams = true;
                                dynamicParameters = new FileSystemProviderRemoveItemDynamicParameters();
                FileSystemInfo fsinfo = GetFileSystemInfo(path, out bool iscontainer);
                if (fsinfo == null)
                    WriteError(new ErrorRecord(e, "ItemDoesNotExist", ErrorCategory.ObjectNotFound, path));
                if (Context != null
                    && Context.ExecutionContext.SessionState.PSVariable.Get(SpecialVariables.ProgressPreferenceVarPath.UserPath).Value is ActionPreference progressPreference
                    && progressPreference == ActionPreference.Continue)
                            GetTotalFiles(path, recurse);
                        _removeStopwatch.Start();
                if (iscontainer)
                    RemoveDirectoryInfoItem((DirectoryInfo)fsinfo, recurse, Force, true);
                    RemoveFileInfoItem((FileInfo)fsinfo, Force);
                if ((!removeStreams) && iscontainer)
                    // If we want to remove the file streams, retrieve them and remove them.
                    if (removeStreams)
                            foreach (AlternateStreamData stream in AlternateDataStreamUtilities.GetStreams(fsinfo.FullName))
                                string action = string.Format(
                                    FileSystemProviderStrings.StreamAction,
                                    stream.Stream, fsinfo.FullName);
                                if (ShouldProcess(action))
                                    AlternateDataStreamUtilities.DeleteFileStream(fsinfo.FullName, stream.Stream);
                                    FileSystemProviderStrings.AlternateDataStreamNotFound, desiredStream, fsinfo.FullName);
                                Exception e = new FileNotFoundException(errorMessage, fsinfo.FullName);
                if (Stopping || _removedFiles == _totalFiles)
                    _removeStopwatch.Stop();
                    var progress = new ProgressRecord(REMOVE_FILE_ACTIVITY_ID, " ", " ")
                WriteError(new ErrorRecord(exception, "RemoveItemIOError", ErrorCategory.WriteError, path));
                WriteError(new ErrorRecord(accessException, "RemoveItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        /// Retrieves the dynamic parameters required for the Remove-Item cmdlet.
        /// <param name="recurse">Whether to recurse into containers.</param>
        /// <returns>An instance of the FileSystemProviderRemoveItemDynamicParameters class that represents the dynamic parameters.</returns>
            if (!recurse)
                return new FileSystemProviderRemoveItemDynamicParameters();
        /// Removes a directory from the file system.
        /// The DirectoryInfo object representing the directory to be removed.
        /// If true, ShouldProcess will be called for each item in the subtree.
        /// If false, ShouldProcess will only be called for the directory item.
        /// If true, attempts to modify the file attributes in case of a failure so that
        /// the file can be removed.
        /// <param name="rootOfRemoval">
        /// True if the DirectoryInfo being passed in is the root of the tree being removed.
        /// ShouldProcess will be called if this is true or if recurse is true.
        private void RemoveDirectoryInfoItem(DirectoryInfo directory, bool recurse, bool force, bool rootOfRemoval)
            Dbg.Diagnostics.Assert(directory != null, "Caller should always check directory");
            bool continueRemoval = true;
            // We only want to confirm the removal if this is the root of the
            // tree being removed or the recurse flag is specified.
            if (rootOfRemoval || recurse)
                // Confirm the user wants to remove the directory
                string action = FileSystemProviderStrings.RemoveItemActionDirectory;
                continueRemoval = ShouldProcess(directory.FullName, action);
            if (InternalSymbolicLinkLinkCodeMethods.IsReparsePointLikeSymlink(directory))
                void WriteErrorHelper(Exception exception)
                    WriteError(new ErrorRecord(exception, errorId: "DeleteSymbolicLinkFailed", ErrorCategory.WriteError, directory));
                    if (InternalTestHooks.OneDriveTestOn)
                        WriteErrorHelper(new IOException());
                        // Name surrogates should just be detached.
                        directory.Delete();
                    string error = StringUtil.Format(FileSystemProviderStrings.CannotRemoveItem, directory.FullName, e.Message);
                    var exception = new IOException(error, e);
                    WriteErrorHelper(exception);
            if (continueRemoval)
                // Loop through each of the contained directories and recurse into them for
                // removal.
                foreach (DirectoryInfo childDir in directory.EnumerateDirectories())
                    if (childDir != null)
                        RemoveDirectoryInfoItem(childDir, recurse, force, false);
                // Loop through each of the contained files and remove them.
                IEnumerable<FileInfo> files = null;
                    files = directory.EnumerateFiles(Filter);
                    files = directory.EnumerateFiles();
                foreach (FileInfo file in files)
                    if (file != null)
                        long fileBytesSize = file.Length;
                            // When recurse is specified we need to confirm each
                            // item before removal.
                            RemoveFileInfoItem(file, force);
                            // When recurse is not specified just delete all the
                            // subitems without confirming with the user.
                            RemoveFileSystemItem(file, force);
                        if (_totalFiles > 0)
                            _removedFiles++;
                            _removedBytes += fileBytesSize;
                            if (_removeStopwatch.Elapsed.TotalSeconds > ProgressBarDurationThreshold)
                                double speed = _removedBytes / 1024 / 1024 / _removeStopwatch.Elapsed.TotalSeconds;
                                var progress = new ProgressRecord(
                                    REMOVE_FILE_ACTIVITY_ID,
                                    StringUtil.Format(FileSystemProviderStrings.RemovingLocalFileActivity, _removedFiles, _totalFiles),
                                    StringUtil.Format(FileSystemProviderStrings.RemovingLocalBytesStatus, Utils.DisplayHumanReadableFileSize(_removedBytes), Utils.DisplayHumanReadableFileSize(_totalBytes), speed)
                                var percentComplete = _totalBytes != 0 ? (int)Math.Min(_removedBytes * 100 / _totalBytes, 100) : 100;
                                progress.PercentComplete = percentComplete;
                // Check to see if the item has children
                bool hasChildren = DirectoryInfoHasChildItems(directory);
                if (hasChildren && !force)
                    string error = StringUtil.Format(FileSystemProviderStrings.DirectoryNotEmpty, directory.FullName);
                    WriteError(new ErrorRecord(e, "DirectoryNotEmpty", ErrorCategory.WriteError, directory));
                else // !hasChildren || force
                    // Finally, remove the directory
                    RemoveFileSystemItem(directory, force);
        /// Removes a file from the file system.
        /// The FileInfo object representing the file to be removed.
        private void RemoveFileInfoItem(FileInfo file, bool force)
                file != null,
                "Caller should always check file");
            string action = FileSystemProviderStrings.RemoveItemActionFile;
            if (ShouldProcess(file.FullName, action))
        /// Removes the file system object from the file system.
        /// <param name="fileSystemInfo">
        /// The FileSystemInfo object representing the file or directory to be removed.
        /// If true, the readonly and hidden attributes will be masked off in the case of
        /// an error, and the removal will be attempted again. If false, exceptions are
        /// written to the error pipeline.
        private void RemoveFileSystemItem(FileSystemInfo fileSystemInfo, bool force)
                fileSystemInfo != null,
                "Caller should always check fileSystemInfo");
            // First check if we can delete this file when force is not specified.
            if (!Force &&
                (fileSystemInfo.Attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly)) != 0)
                string error = StringUtil.Format(FileSystemProviderStrings.PermissionError);
                ErrorDetails errorDetails =
                    new ErrorDetails(this, "FileSystemProviderStrings",
                        "CannotRemoveItem",
                        fileSystemInfo.FullName,
                ErrorRecord errorRecord = new ErrorRecord(e, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, fileSystemInfo);
            // Store the old attributes in case we fail to delete
            FileAttributes oldAttributes = fileSystemInfo.Attributes;
            bool attributeRecoveryRequired = false;
                // Try to delete the item.  Strip any problematic attributes
                // if they've specified force.
                    fileSystemInfo.Attributes &= ~(FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
                    attributeRecoveryRequired = true;
                fileSystemInfo.Delete();
                    attributeRecoveryRequired = false;
            catch (Exception fsException)
                        fsException.Message);
                if ((fsException is System.Security.SecurityException) ||
                    (fsException is UnauthorizedAccessException))
                    ErrorRecord errorRecord = new ErrorRecord(fsException, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, fileSystemInfo);
                else if (fsException is ArgumentException)
                    ErrorRecord errorRecord = new ErrorRecord(fsException, "RemoveFileSystemItemArgumentError", ErrorCategory.InvalidArgument, fileSystemInfo);
                else if ((fsException is IOException) ||
                    (fsException is FileNotFoundException) ||
                    (fsException is DirectoryNotFoundException))
                    ErrorRecord errorRecord = new ErrorRecord(fsException, "RemoveFileSystemItemIOError", ErrorCategory.WriteError, fileSystemInfo);
                if (attributeRecoveryRequired)
                        if (fileSystemInfo.Exists)
                            fileSystemInfo.Attributes = oldAttributes;
                    catch (Exception attributeException)
                        if ((attributeException is System.IO.DirectoryNotFoundException) ||
                            (attributeException is System.Security.SecurityException) ||
                            (attributeException is System.ArgumentException) ||
                            (attributeException is System.IO.FileNotFoundException) ||
                            (attributeException is System.IO.IOException))
                            ErrorDetails attributeDetails = new ErrorDetails(
                                this, "FileSystemProviderStrings",
                                    "CannotRestoreAttributes",
                                    attributeException.Message);
                            ErrorRecord errorRecord = new ErrorRecord(attributeException, "RemoveFileSystemItemCannotRestoreAttributes", ErrorCategory.PermissionDenied, fileSystemInfo);
                            errorRecord.ErrorDetails = attributeDetails;
        /// Determines if a file or directory exists at the specified path.
        /// True if a file or directory exists at the specified path, false otherwise.
            bool result = ItemExists(path, out error);
        /// Implementation of ItemExists for the provider. This implementation
        /// allows the caller to decide if it wants to WriteError or not based
        /// on the returned ErrorRecord.
        /// The path of the object to check
        /// An error record is returned in this parameter if there was an error.
        /// True if an object exists at the specified path, false otherwise.
        private bool ItemExists(string path, out ErrorRecord error)
                var fsinfo = GetFileSystemInfo(path, out bool _);
                result = fsinfo != null;
                FileSystemItemProviderDynamicParameters itemExistsDynamicParameters =
                    DynamicParameters as FileSystemItemProviderDynamicParameters;
                // If the items see if we need to check the age of the file...
                if (result && itemExistsDynamicParameters != null)
                    DateTime lastWriteTime = fsinfo.LastWriteTime;
                    if (itemExistsDynamicParameters.OlderThan.HasValue)
                        result &= lastWriteTime < itemExistsDynamicParameters.OlderThan.Value;
                    if (itemExistsDynamicParameters.NewerThan.HasValue)
                        result &= lastWriteTime > itemExistsDynamicParameters.NewerThan.Value;
            catch (System.Security.SecurityException security)
                error = new ErrorRecord(security, "ItemExistsSecurityError", ErrorCategory.PermissionDenied, path);
            catch (ArgumentException argument)
                error = new ErrorRecord(argument, "ItemExistsArgumentError", ErrorCategory.InvalidArgument, path);
                error = new ErrorRecord(unauthorized, "ItemExistsUnauthorizedAccessError", ErrorCategory.PermissionDenied, path);
                error = new ErrorRecord(pathTooLong, "ItemExistsPathTooLongError", ErrorCategory.InvalidArgument, path);
                error = new ErrorRecord(notSupported, "ItemExistsNotSupportedError", ErrorCategory.InvalidOperation, path);
        /// Adds -OlderThan, -NewerThan dynamic properties.
        protected override object ItemExistsDynamicParameters(string path)
                return new FileSystemItemProviderDynamicParameters();
        /// Determines if the given path is a directory, and has children.
        /// The full path to the directory.
        /// True if the path refers to a directory that contains other
        /// directories or files.  False otherwise.
            // First check to see if it is a directory
                DirectoryInfo directory = new DirectoryInfo(path);
                // If the above didn't throw an exception, check to
                // see if we should proceed and if it contains any children
                if ((directory.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                result = DirectoryInfoHasChildItems(directory);
                // Since we couldn't convert the path to a DirectoryInfo
                // the path could not be a file system container with
                // Happens when we try to access an alternate data stream
        private static bool DirectoryInfoHasChildItems(DirectoryInfo directory)
                directory != null,
                "The caller should verify directory.");
            IEnumerable<FileSystemInfo> children = directory.EnumerateFileSystemInfos();
            if (children.Any())
        /// Copies an item at the specified path to the given destination.
        /// Specifies if the operation should also copy child items.
        ///     destination path is null or empty.
        /// Nothing.  Copied items are written to the context's pipeline.
        protected override void CopyItem(
            if (string.IsNullOrEmpty(destinationPath))
                throw PSTraceSource.NewArgumentException(nameof(destinationPath));
            destinationPath = NormalizePath(destinationPath);
            PSSession fromSession = null;
            PSSession toSession = null;
            CopyItemDynamicParameters copyDynamicParameter = DynamicParameters as CopyItemDynamicParameters;
            if (copyDynamicParameter != null)
                if (copyDynamicParameter.FromSession != null)
                    fromSession = copyDynamicParameter.FromSession;
                    toSession = copyDynamicParameter.ToSession;
            _excludeMatcher = SessionStateUtilities.CreateWildcardsFromStrings(Exclude, WildcardOptions.IgnoreCase);
            // if the source and destination path are same (for a local copy) then flag it as error.
            if ((toSession == null) && (fromSession == null) && InternalSymbolicLinkLinkCodeMethods.IsSameFileSystemItem(path, destinationPath))
                string error = StringUtil.Format(FileSystemProviderStrings.CopyError, path);
                e.Data[SelfCopyDataKey] = destinationPath;
                WriteError(new ErrorRecord(e, "CopyError", ErrorCategory.WriteError, path));
            // Copy-Item from session
            if (fromSession != null)
                CopyItemFromRemoteSession(path, destinationPath, recurse, Force, fromSession);
                // Copy-Item to session
                if (toSession != null)
                        ps.Runspace = toSession.Runspace;
                        CopyItemLocalOrToSession(path, destinationPath, recurse, Force, ps);
                else // Copy-Item local
                    if (Context != null && Context.ExecutionContext.SessionState.PSVariable.Get(SpecialVariables.ProgressPreferenceVarPath.UserPath).Value is ActionPreference progressPreference && progressPreference == ActionPreference.Continue)
                            _copyStopwatch.Start();
                    CopyItemLocalOrToSession(path, destinationPath, recurse, Force, null);
                    if (Stopping || _copiedFiles == _totalFiles)
                        _copyStopwatch.Stop();
                        var progress = new ProgressRecord(COPY_FILE_ACTIVITY_ID, " ", " ");
            _excludeMatcher.Clear();
            _excludeMatcher = null;
        private void GetTotalFiles(string path, bool recurse)
                    var enumOptions = new EnumerationOptions()
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = recurse
                    var directory = new DirectoryInfo(path);
                    foreach (var file in directory.EnumerateFiles("*", enumOptions))
                        if (!SessionStateUtilities.MatchesAnyWildcardPattern(file.Name, _excludeMatcher, defaultValue: false))
                            _totalFiles++;
                            _totalBytes += file.Length;
                    var file = new FileInfo(path);
                // ignore exception
        private void CopyItemFromRemoteSession(string path, string destinationPath, bool recurse, bool force, PSSession fromSession)
                ps.Runspace = fromSession.Runspace;
                InitializeFunctionPSCopyFileFromRemoteSession(ps);
                    // get info on source
                    ps.AddCommand(CopyFileRemoteUtils.PSCopyFromSessionHelperName);
                    ps.AddParameter("getPathItems", path);
                    Hashtable op = SafeInvokeCommand.Invoke(ps, this, null);
                        Exception e = new IOException(string.Format(CultureInfo.InvariantCulture, FileSystemProviderStrings.CopyItemRemotelyFailedToReadFile, path));
                        WriteError(new ErrorRecord(e, "CopyItemRemotelyFailedToReadFile", ErrorCategory.WriteError, path));
                    bool exists = (bool)(op["Exists"]);
                        throw PSTraceSource.NewArgumentNullException(SessionStateStrings.PathNotFound, path);
                    if (op["Items"] != null)
                        PSObject obj = (PSObject)op["Items"];
                        ArrayList itemsList = (ArrayList)obj.BaseObject;
                        foreach (PSObject item in itemsList)
                            Hashtable ItemInfo = (Hashtable)item.BaseObject;
                            string itemName = (string)ItemInfo["Name"];
                            string itemFullName = (string)ItemInfo["FullName"];
                            bool isContainer = (bool)ItemInfo["IsDirectory"];
                                if (File.Exists(destinationPath))
                                    Exception e = new IOException(string.Format(
                                        FileSystemProviderStrings.CopyItemRemotelyDestinationIsFile,
                                        destinationPath));
                                    WriteError(new ErrorRecord(e, "CopyError", ErrorCategory.WriteError, destinationPath));
                                CopyDirectoryFromRemoteSession(
                                    itemName,
                                    itemFullName,
                                    ps);
                                bool excludeFile = SessionStateUtilities.MatchesAnyWildcardPattern(itemName, _excludeMatcher, false);
                                if (!excludeFile)
                                    long itemSize = (long)ItemInfo["FileSize"];
                                    CopyFileFromRemoteSession(itemName, itemFullName, destinationPath, force, ps, itemSize);
                    RemoveFunctionsPSCopyFileFromRemoteSession(ps);
        private void CopyItemLocalOrToSession(string path, string destinationPath, bool recurse, bool Force, System.Management.Automation.PowerShell ps)
            InitializeFunctionsPSCopyFileToRemoteSession(ps);
                    // Get the directory info
                    // Now copy the directory to the destination
                    CopyDirectoryInfoItem(dir, destinationPath, recurse, Force, ps);
                else // !isContainer
                    // Get the file info
                    CopyFileInfoItem(file, destinationPath, Force, ps);
                RemoveFunctionPSCopyFileToRemoteSession(ps);
        private void CopyDirectoryInfoItem(
            System.Management.Automation.PowerShell ps)
            // Generate the path based on whether the destination path exists and
            // is a container.
            // If the destination exists and is a container the directory we are copying
            // will become a child of that directory.
            // If the destination doesn't exist we will just try to copy to that new
            if (ps == null)
                if (IsItemContainer(destination))
                    destination = MakePath(destination, directory.Name);
                if (RemoteDirectoryExist(ps, destination))
                    destination = Path.Combine(destination, directory.Name);
            s_tracer.WriteLine("destination = {0}", destination);
            // Confirm the copy with the user
            string action = FileSystemProviderStrings.CopyItemActionDirectory;
            string resource = StringUtil.Format(FileSystemProviderStrings.CopyItemResourceFileTemplate, directory.FullName, destination);
                // Create the new directory
                // CreateDirectory does the WriteItemObject for the new DirectoryInfo
                    CreateDirectory(destination, true);
                    // Verify that the destination is not a file on the remote end
                    if (RemoteDestinationPathIsFile(destination, ps))
                        Exception e = new IOException(string.Format(CultureInfo.InvariantCulture,
                                                                    FileSystemProviderStrings.CopyItemRemoteDestinationIsFile,
                                                                    destination));
                        WriteError(new ErrorRecord(e, "CopyError", ErrorCategory.WriteError, destination));
                    destination = CreateDirectoryOnRemoteSession(destination, force, ps);
                    // Now copy all the files to that directory
                    if (string.IsNullOrEmpty(Filter))
                                // CopyFileInfoItem does the WriteItemObject for the new FileInfo
                                CopyFileInfoItem(file, destination, force, ps);
                                WriteError(new ErrorRecord(argException, "CopyDirectoryInfoItemArgumentError", ErrorCategory.InvalidArgument, file));
                                WriteError(new ErrorRecord(ioException, "CopyDirectoryInfoItemIOError", ErrorCategory.WriteError, file));
                                WriteError(new ErrorRecord(accessException, "CopyDirectoryInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, file));
                    // Now copy all the directories to that directory
                                CopyDirectoryInfoItem(childDir, destination, recurse, force, ps);
                                WriteError(new ErrorRecord(argException, "CopyDirectoryInfoItemArgumentError", ErrorCategory.InvalidArgument, childDir));
                                WriteError(new ErrorRecord(ioException, "CopyDirectoryInfoItemIOError", ErrorCategory.WriteError, childDir));
                                WriteError(new ErrorRecord(accessException, "CopyDirectoryInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, childDir));
        private void CopyFileInfoItem(FileInfo file, string destinationPath, bool force, System.Management.Automation.PowerShell ps)
                "The caller should verify file.");
            // If the destination is a container, add the file name
            // to the destination path.
                if (IsItemContainer(destinationPath))
                    destinationPath = MakePath(destinationPath, file.Name);
                // if the source and destination path are same then flag it as error.
                if (InternalSymbolicLinkLinkCodeMethods.IsSameFileSystemItem(destinationPath, file.FullName))
                    string error = StringUtil.Format(FileSystemProviderStrings.CopyError, destinationPath);
                    e.Data[SelfCopyDataKey] = file.FullName;
                if (PathIsReservedDeviceName(destinationPath, "CopyError"))
            string action = FileSystemProviderStrings.CopyItemActionFile;
            string resource = StringUtil.Format(FileSystemProviderStrings.CopyItemResourceFileTemplate, file.FullName, destinationPath);
            bool excludeFile = SessionStateUtilities.MatchesAnyWildcardPattern(file.Name, _excludeMatcher, defaultValue: false);
                            // Now copy the file
                            // We assume that if we get called we want to make
                            // the copy even if the destination already exists.
                            file.CopyTo(destinationPath, true);
                            FileInfo result = new FileInfo(destinationPath);
                            WriteItemObject(result, destinationPath, false);
                                _copiedFiles++;
                                _copiedBytes += file.Length;
                                if (_copyStopwatch.Elapsed.TotalSeconds > ProgressBarDurationThreshold)
                                    double speed = (double)(_copiedBytes / 1024 / 1024) / _copyStopwatch.Elapsed.TotalSeconds;
                                        COPY_FILE_ACTIVITY_ID,
                                        StringUtil.Format(FileSystemProviderStrings.CopyingLocalFileActivity, _copiedFiles, _totalFiles),
                                        StringUtil.Format(FileSystemProviderStrings.CopyingLocalBytesStatus, Utils.DisplayHumanReadableFileSize(_copiedBytes), Utils.DisplayHumanReadableFileSize(_totalBytes), speed)
                                    var percentComplete = _totalBytes != 0 ? (int)Math.Min(_copiedBytes * 100 / _totalBytes, 100) : 100;
                            PerformCopyFileToRemoteSession(file, destinationPath, ps);
                    catch (System.UnauthorizedAccessException unAuthorizedAccessException)
                                    // If the destination exists and force is specified,
                                    // mask of the readonly and hidden attributes and
                                    // try again
                                    FileInfo destinationItem = new FileInfo(destinationPath);
                                    destinationItem.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
                                    // Write out the original error since we failed to force the copy
                                    WriteError(new ErrorRecord(unAuthorizedAccessException, "CopyFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, file));
                        WriteError(new ErrorRecord(ioException, "CopyFileInfoItemIOError", ErrorCategory.WriteError, file));
        private void CopyDirectoryFromRemoteSession(
            string sourceDirectoryName,
            string sourceDirectoryFullName,
            Dbg.Diagnostics.Assert((sourceDirectoryName != null && sourceDirectoryFullName != null), "The caller should verify directory.");
                destination = MakePath(destination, sourceDirectoryName);
            string resource = StringUtil.Format(FileSystemProviderStrings.CopyItemResourceFileTemplate, sourceDirectoryFullName, destination);
                // Create destinationPath directory. This will fail if the directory already exists
                // and Force is not selected.
                CreateDirectory(destination, false);
                // If failed to create directory
                if (!Directory.Exists(destination))
                    // Get all the files for that directory from the remote session
                    ps.AddParameter("getPathDir", sourceDirectoryFullName);
                                                      FileSystemProviderStrings.CopyItemRemotelyFailedToGetDirectoryChildItems,
                                                      sourceDirectoryFullName));
                        WriteError(new ErrorRecord(e, "CopyError", ErrorCategory.WriteError, sourceDirectoryFullName));
                    if (op["Files"] != null)
                        PSObject obj = (PSObject)op["Files"];
                        ArrayList filesList = (ArrayList)obj.BaseObject;
                        foreach (PSObject fileObject in filesList)
                            Hashtable file = (Hashtable)fileObject.BaseObject;
                            string fileName = (string)file["FileName"];
                            string filePath = (string)file["FilePath"];
                            long fileSize = (long)file["FileSize"];
                            bool excludeFile = SessionStateUtilities.MatchesAnyWildcardPattern(fileName, _excludeMatcher, defaultValue: false);
                                // If an exception is thrown in the remote session, it is surface to the user via PowerShell Write-Error.
                                CopyFileFromRemoteSession(fileName,
                                                          ps,
                                                          fileSize);
                    if (op["Directories"] != null)
                        PSObject obj = (PSObject)op["Directories"];
                        ArrayList directories = (ArrayList)obj.BaseObject;
                        foreach (PSObject dirObject in directories)
                            Hashtable dir = (Hashtable)dirObject.BaseObject;
                            string dirName = (string)dir["Name"];
                            string dirFullName = (string)dir["FullName"];
                            CopyDirectoryFromRemoteSession(dirName,
                                                           dirFullName,
        private ArrayList GetRemoteSourceAlternateStreams(System.Management.Automation.PowerShell ps, string path)
            ArrayList streams = null;
            bool supportsAlternateStreams = false;
            ps.AddParameter("supportAltStreamPath", path);
            if (op != null && op["SourceSupportsAlternateStreams"] != null)
                supportsAlternateStreams = (bool)op["SourceSupportsAlternateStreams"];
            if (supportsAlternateStreams)
                PSObject obj = (PSObject)op["Streams"];
                streams = (ArrayList)obj.BaseObject;
            return streams;
        private void InitializeFunctionPSCopyFileFromRemoteSession(System.Management.Automation.PowerShell ps)
            if ((ps == null) || !ValidRemoteSessionForScripting(ps.Runspace))
            ps.AddScript(CopyFileRemoteUtils.AllCopyFromRemoteScripts);
            SafeInvokeCommand.Invoke(ps, this, null, false);
        private void RemoveFunctionsPSCopyFileFromRemoteSession(System.Management.Automation.PowerShell ps)
            const string remoteScript = @"
                Microsoft.PowerShell.Management\Remove-Item function:PSCopyFromSessionHelper -ea SilentlyContinue -Force
                Microsoft.PowerShell.Management\Remove-Item function:PSCopyRemoteUtils -ea SilentlyContinue -Force
            ps.AddScript(remoteScript);
        private static bool ValidRemoteSessionForScripting(Runspace runspace)
            if (runspace is not RemoteRunspace)
            PSLanguageMode languageMode = runspace.SessionStateProxy.LanguageMode;
            if (languageMode == PSLanguageMode.ConstrainedLanguage || languageMode == PSLanguageMode.NoLanguage)
                // SessionStateInternal.ValidateRemotePathAndGetRoot checked for expected helper functions on the
                // restricted session and will have returned an error if they are missing.  So at this point we
                // assume the session is set up with the needed helper functions.
        private Hashtable GetRemoteFileMetadata(string filePath, System.Management.Automation.PowerShell ps)
            ps.AddParameter("getMetaFilePath", filePath);
            Hashtable metadata = SafeInvokeCommand.Invoke(ps, this, null);
        private void SetFileMetadata(string sourceFileFullName, FileInfo destinationFile, System.Management.Automation.PowerShell ps)
            Hashtable metadata = GetRemoteFileMetadata(sourceFileFullName, ps);
            if (metadata != null)
                // LastWriteTime
                if (metadata["LastWriteTimeUtc"] != null)
                    destinationFile.LastWriteTimeUtc = (DateTime)metadata["LastWriteTimeUtc"];
                if (metadata["LastWriteTime"] != null)
                    destinationFile.LastWriteTime = (DateTime)metadata["LastWriteTime"];
                // Attributes
                if (metadata["Attributes"] != null)
                    PSObject obj = (PSObject)metadata["Attributes"];
                    foreach (string value in (ArrayList)obj.BaseObject)
                        if (string.Equals(value, "ReadOnly", StringComparison.OrdinalIgnoreCase))
                            destinationFile.Attributes |= FileAttributes.ReadOnly;
                        else if (string.Equals(value, "Hidden", StringComparison.OrdinalIgnoreCase))
                            destinationFile.Attributes |= FileAttributes.Hidden;
                        else if (string.Equals(value, "Archive", StringComparison.OrdinalIgnoreCase))
                            destinationFile.Attributes |= FileAttributes.Archive;
                        else if (string.Equals(value, "System", StringComparison.OrdinalIgnoreCase))
                            destinationFile.Attributes |= FileAttributes.System;
        private void CopyFileFromRemoteSession(
            string sourceFileName,
            string sourceFileFullName,
            System.Management.Automation.PowerShell ps,
            long fileSize = 0)
            Dbg.Diagnostics.Assert(sourceFileFullName != null, "The caller should verify file.");
                destinationPath = MakePath(destinationPath, sourceFileName);
            FileInfo destinationFile = new FileInfo(destinationPath);
            string resource = StringUtil.Format(FileSystemProviderStrings.CopyItemResourceFileTemplate, sourceFileFullName, destinationPath);
                bool result = PerformCopyFileFromRemoteSession(sourceFileFullName, destinationFile, destinationPath, force, ps, fileSize, false, null);
                // Copying the file from the remote session completed successfully
                    // Check if the remote source file has any alternate data streams
                    ArrayList remoteFileStreams = GetRemoteSourceAlternateStreams(ps, sourceFileFullName);
                    if ((remoteFileStreams != null) && (remoteFileStreams.Count > 0))
                        foreach (string streamName in remoteFileStreams)
                            result = PerformCopyFileFromRemoteSession(sourceFileFullName, destinationFile, destinationPath, force, ps, fileSize, true, streamName);
                // The file was copied successfully. Now, set the file metadata
                    SetFileMetadata(sourceFileFullName, destinationFile, ps);
        private bool PerformCopyFileFromRemoteSession(string sourceFileFullName, FileInfo destinationFile, string destinationPath, bool force, System.Management.Automation.PowerShell ps,
                                                      long fileSize, bool isAlternateDataStream, string streamName)
            string activity = string.Format(CultureInfo.InvariantCulture,
                                            FileSystemProviderStrings.CopyItemRemotelyProgressActivity,
                                            sourceFileFullName,
                                            destinationFile.FullName);
            string statusDescription = string.Format(CultureInfo.InvariantCulture,
                                                        FileSystemProviderStrings.CopyItemRemotelyStatusDescription,
                                                        ps.Runspace.ConnectionInfo.ComputerName,
                                                        "localhost");
            ProgressRecord progress = new ProgressRecord(0, activity, statusDescription);
            FileStream wStream = null;
            bool errorWhileCopyRemoteFile = false;
                // The main data stream
                if (!isAlternateDataStream)
                    // If force is specified, and the file already exist at the destination, mask of the readonly, hidden, and system attributes
                    if (force && File.Exists(destinationFile.FullName))
                        destinationFile.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);
                    wStream = new FileStream(destinationFile.FullName, FileMode.Create);
                // an alternate stream
                    wStream = AlternateDataStreamUtilities.CreateFileStream(destinationFile.FullName, streamName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                const long fragmentSize = FILETRANSFERSIZE;
                long copiedSoFar = 0;
                long currentIndex = 0;
                    ps.AddParameter("copyFromFilePath", sourceFileFullName);
                    ps.AddParameter("copyFromStart", currentIndex);
                    ps.AddParameter("copyFromNumBytes", fragmentSize);
                        ps.AddParameter(nameof(force), true);
                    if (isAlternateDataStream)
                        ps.AddParameter("isAlternateStream", true);
                        ps.AddParameter(nameof(streamName), streamName);
                    // Check if there was an exception when reading the remote file.
                        errorWhileCopyRemoteFile = true;
                                                                    FileSystemProviderStrings.CopyItemRemotelyFailedToReadFile,
                                                                    sourceFileFullName));
                        WriteError(new ErrorRecord(e, "FailedToCopyFileFromRemoteSession", ErrorCategory.WriteError, sourceFileFullName));
                    if (op["ExceptionThrown"] != null)
                        bool failedToReadFile = (bool)(op["ExceptionThrown"]);
                        if (failedToReadFile)
                            // The error is written to the error array via SafeInvokeCommand
                    // To accommodate empty files
                    string content = string.Empty;
                    if (op["b64Fragment"] != null)
                        content = (string)op["b64Fragment"];
                    bool more = (bool)op["moreAvailable"];
                    currentIndex += fragmentSize;
                    byte[] bytes = System.Convert.FromBase64String(content);
                    wStream.Write(bytes, 0, bytes.Length);
                    copiedSoFar += bytes.Length;
                    if (wStream.Length > 0)
                        int percentage = (int)(copiedSoFar * 100 / wStream.Length);
                        progress.PercentComplete = percentage;
                    if (!more)
                WriteError(new ErrorRecord(ioException, "CopyItemRemotelyIOError", ErrorCategory.WriteError, sourceFileFullName));
                WriteError(new ErrorRecord(argException, "CopyItemRemotelyArgumentError", ErrorCategory.WriteError, sourceFileFullName));
                WriteError(new ErrorRecord(notSupportedException, "CopyFileInfoRemotelyPathRefersToANonFileDevice", ErrorCategory.InvalidArgument, sourceFileFullName));
                WriteError(new ErrorRecord(securityException, "CopyFileInfoRemotelyUnauthorizedAccessError", ErrorCategory.PermissionDenied, sourceFileFullName));
                WriteError(new ErrorRecord(unauthorizedAccessException, "CopyFileInfoItemRemotelyUnauthorizedAccessError", ErrorCategory.PermissionDenied, sourceFileFullName));
                wStream?.Dispose();
                // If copying the file from the remote session failed, then remove it.
                if (errorWhileCopyRemoteFile && File.Exists(destinationFile.FullName))
                    if (!(destinationFile.Attributes.HasFlag(FileAttributes.ReadOnly) ||
                            destinationFile.Attributes.HasFlag(FileAttributes.Hidden) ||
                            destinationFile.Attributes.HasFlag(FileAttributes.System)))
                        RemoveFileSystemItem(destinationFile, true);
        private void InitializeFunctionsPSCopyFileToRemoteSession(System.Management.Automation.PowerShell ps)
            ps.AddScript(CopyFileRemoteUtils.AllCopyToRemoteScripts);
        private void RemoveFunctionPSCopyFileToRemoteSession(System.Management.Automation.PowerShell ps)
                Microsoft.PowerShell.Management\Remove-Item function:PSCopyToSessionHelper -ea SilentlyContinue -Force
        // If the target supports alternate data streams the following must be true:
        // 1) The remote session must be PowerShell V3 or higher to support Streams
        // 2) The target drive must be NTFS
        private bool RemoteTargetSupportsAlternateStreams(System.Management.Automation.PowerShell ps, string path)
            ps.AddCommand(CopyFileRemoteUtils.PSCopyToSessionHelperName);
            if (op != null && op["TargetSupportsAlternateStreams"] != null)
                supportsAlternateStreams = (bool)op["TargetSupportsAlternateStreams"];
            return supportsAlternateStreams;
        // Validate that the given remotePath exists, and do the following:
        // 1) If the remotePath is a FileInfo, then just return the remotePath.
        // 2) If the remotePath is a DirectoryInfo, then return the remotePath + the given filename.
        // 3) If the remote path does not exist, but its parent does, and it is a DirectoryInfo, then return the remotePath.
        // 4) If the remotePath or its parent do not exist, return null.
        private string MakeRemotePath(System.Management.Automation.PowerShell ps, string remotePath, string filename)
            bool isFileInfo = false;
            bool isDirectoryInfo = false;
            bool parentIsDirectoryInfo = false;
            ps.AddParameter(nameof(remotePath), remotePath);
            if (op != null)
                if (op["IsDirectoryInfo"] != null)
                    isDirectoryInfo = (bool)op["IsDirectoryInfo"];
                if (op["IsFileInfo"] != null)
                    isFileInfo = (bool)op["IsFileInfo"];
                if (op["ParentIsDirectoryInfo"] != null)
                    parentIsDirectoryInfo = (bool)op["ParentIsDirectoryInfo"];
            if (isFileInfo)
                // The destination is a file, so we are going to overwrite it.
                path = remotePath;
            else if (isDirectoryInfo)
                // The destination is a directory, so append the file name to the path.
                path = Path.Combine(remotePath, filename);
            else if (parentIsDirectoryInfo)
                // At this point we know that the remotePath is neither a file or a directory on the remote target.
                // However, if the parent of the remotePath exists, then we are doing a copy-item operation in which
                // the destination file name is already being passed, e.g.,
                // copy-item -path c:\localDir\foo.txt -destination d:\remoteDir\bar.txt -toSession $s
                // Note that d:\remoteDir is a directory that exists on the remote target machine.
        private bool RemoteDirectoryExist(System.Management.Automation.PowerShell ps, string path)
            bool pathExists = false;
            ps.AddParameter("dirPathExists", path);
                    pathExists = (bool)op["Exists"];
            return pathExists;
        private bool CopyFileStreamToRemoteSession(FileInfo file, string destinationPath, System.Management.Automation.PowerShell ps, bool isAlternateStream, string streamName)
                                            file.FullName,
                                            destinationPath);
                                                     "localhost",
                                                     ps.Runspace.ConnectionInfo.ComputerName);
            // 4MB gives the best results without spiking the resources on the remote connection.
            const int fragmentSize = FILETRANSFERSIZE;
            byte[] fragment = null;
            int iteration = 0;
            FileStream fStream = null;
                // Main data stream
                if (!isAlternateStream)
                    fStream = File.OpenRead(file.FullName);
                    fStream = AlternateDataStreamUtilities.CreateFileStream(file.FullName, streamName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long remainingFileSize = fStream != null ? fStream.Length : 0;
                    iteration++;
                    int toRead = fragmentSize;
                    if (toRead > remainingFileSize)
                        toRead = (int)remainingFileSize;
                    if (fragment == null)
                        fragment = new byte[toRead];
                    else if (toRead < fragmentSize)
                    int readSoFar = 0;
                    while (readSoFar < toRead)
                        readSoFar += fStream.Read(fragment, 0, toRead);
                    remainingFileSize -= readSoFar;
                    string b64Fragment = System.Convert.ToBase64String(fragment);
                        ps.AddParameter("copyToFilePath", destinationPath);
                        ps.AddParameter("createFile", (iteration == 1));
                        if ((iteration == 1) && (b64Fragment.Length == 0))
                            // This fixes the case in which the user tries to copy an empty file between sessions.
                            // Scenario 1: The user creates an empty file using the Out-File cmdlet.
                            //             In this case the file length is 6.
                            //             "" | out-file test.txt
                            // Scenario 2: The user generates an empty file using the New-Item cmdlet.
                            //             In this case the file length is 0.
                            //             New-Item -Path test.txt -Type File
                            // Because of this, when we create the file on the remote session, we need to check
                            // the length of b64Fragment to figure out if we are creating an empty file.
                            ps.AddParameter("emptyFile", true);
                            ps.AddParameter("b64Fragment", b64Fragment);
                    if (op == null || op["BytesWritten"] == null)
                        // write error to stream
                        Exception e = new IOException(string.Format(CultureInfo.InvariantCulture, FileSystemProviderStrings.CopyItemRemotelyFailed, file));
                        WriteError(new ErrorRecord(e, "CopyError", ErrorCategory.WriteError, file.FullName));
                    if ((int)(op["BytesWritten"]) != toRead)
                    if (fStream.Length > 0)
                        int percentage = (int)((fStream.Length - remainingFileSize) * 100 / fStream.Length);
                } while (remainingFileSize > 0);
                WriteError(new ErrorRecord(ioException, "CopyItemRemotelyIOError", ErrorCategory.WriteError, file.FullName));
                WriteError(new ErrorRecord(argException, "CopyItemRemotelyArgumentError", ErrorCategory.WriteError, file.FullName));
                WriteError(new ErrorRecord(notSupportedException, "CopyFileInfoRemotelyPathRefersToANonFileDevice", ErrorCategory.InvalidArgument, file.FullName));
                WriteError(new ErrorRecord(securityException, "CopyFileInfoRemotelyUnauthorizedAccessError", ErrorCategory.PermissionDenied, file.FullName));
                fStream?.Dispose();
        // Returns a hash table with metadata about this file info.
        private static Hashtable GetFileMetadata(FileInfo file)
            Hashtable metadata = new Hashtable();
            metadata.Add("LastWriteTime", file.LastWriteTime);
            metadata.Add("LastWriteTimeUtc", file.LastWriteTimeUtc);
            // File attributes
            metadata.Add("Attributes", file.Attributes);
        private void SetRemoteFileMetadata(FileInfo file, string remoteFilePath, System.Management.Automation.PowerShell ps)
            Hashtable metadata = GetFileMetadata(file);
                ps.AddParameter("metaDataFilePath", remoteFilePath);
                ps.AddParameter("metaDataToSet", metadata);
        private bool PerformCopyFileToRemoteSession(FileInfo file, string destinationPath, System.Management.Automation.PowerShell ps)
            // Make the remote path
            var remoteFilePath = MakeRemotePath(ps, destinationPath, file.Name);
            if (string.IsNullOrEmpty(remoteFilePath))
                Exception e = new ArgumentException(string.Format(CultureInfo.InvariantCulture, SessionStateStrings.PathNotFound, destinationPath));
                WriteError(new ErrorRecord(e, "RemotePathNotFound", ErrorCategory.WriteError, destinationPath));
            bool result = CopyFileStreamToRemoteSession(file, remoteFilePath, ps, false, null);
            bool targetSupportsAlternateStreams = RemoteTargetSupportsAlternateStreams(ps, remoteFilePath);
            // Once the file is copied successfully, check if the file has any alternate data streams
            if (result && targetSupportsAlternateStreams)
                foreach (AlternateStreamData stream in AlternateDataStreamUtilities.GetStreams(file.FullName))
                    if (!(string.Equals(":$DATA", stream.Stream, StringComparison.OrdinalIgnoreCase)))
                        result = CopyFileStreamToRemoteSession(file, remoteFilePath, ps, true, stream.Stream);
                SetRemoteFileMetadata(file, Path.Combine(destinationPath, file.Name), ps);
        private bool RemoteDestinationPathIsFile(string destination, System.Management.Automation.PowerShell ps)
            ps.AddParameter("isFilePath", destination);
            if (op == null || op["IsFileInfo"] == null)
                                                    FileSystemProviderStrings.CopyItemRemotelyFailedToValidateIfDestinationIsFile,
            return (bool)(op["IsFileInfo"]);
        private string CreateDirectoryOnRemoteSession(string destination, bool force, System.Management.Automation.PowerShell ps)
            ps.AddParameter("createDirectoryPath", destination);
            // If op == null,  SafeInvokeCommand.Invoke throwns an error.
                // If an error is thrown on the remote session, it is written via SafeInvokeCommand.Invoke.
                if ((bool)op["ExceptionThrown"])
            if (force && (op["DirectoryPath"] == null))
                                                            FileSystemProviderStrings.CopyItemRemotelyFailedToCreateDirectory,
                WriteError(new ErrorRecord(e, "FailedToCreateDirectory", ErrorCategory.WriteError, destination));
            string path = (string)(op["DirectoryPath"]);
            if ((!force) && (bool)op["PathExists"])
                Exception e = new IOException(StringUtil.Format(FileSystemProviderStrings.DirectoryExist, path));
                WriteError(new ErrorRecord(e, "DirectoryExist", ErrorCategory.ResourceExists, path));
        // Returns true if the destination path represents a device name, and write an error to the user.
        private bool PathIsReservedDeviceName(string destinationPath, string errorId)
            bool pathIsReservedDeviceName = false;
            if (Utils.IsReservedDeviceName(destinationPath))
                pathIsReservedDeviceName = true;
                string error = StringUtil.Format(FileSystemProviderStrings.TargetCannotContainDeviceName, destinationPath);
                WriteError(new ErrorRecord(e, errorId, ErrorCategory.WriteError, destinationPath));
            return pathIsReservedDeviceName;
        private long _totalFiles;
        private long _totalBytes;
        private long _copiedFiles;
        private long _copiedBytes;
        private readonly Stopwatch _copyStopwatch = new Stopwatch();
        private long _removedBytes;
        private long _removedFiles;
        private readonly Stopwatch _removeStopwatch = new();
        private const double ProgressBarDurationThreshold = 2.0;
        #endregion ContainerCmdletProvider members
        #region NavigationCmdletProvider members
            if (!Utils.PathIsUnc(path))
                parentPath = EnsureDriveIsRooted(parentPath);
            else if (parentPath.Equals(StringLiterals.DefaultPathSeparatorString, StringComparison.Ordinal))
                // make sure we return two backslashes so it still results in a UNC path
                parentPath = "\\\\";
            if (!parentPath.EndsWith(StringLiterals.DefaultPathSeparator)
                && Utils.PathIsDevicePath(parentPath)
                && parentPath.Length - parentPath.Replace(StringLiterals.DefaultPathSeparatorString, string.Empty).Length == 3)
                // Device paths start with either "\\.\" or "\\?\"
                // When referring to the root, like: "\\.\CDROM0\" then it needs the trailing separator to be valid.
                parentPath += StringLiterals.DefaultPathSeparator;
            s_tracer.WriteLine("GetParentPath returning '{0}'", parentPath);
        // Note: we don't use IO.Path.IsPathRooted as this deals with "invalid" i.e. unnormalized paths
        private static bool IsAbsolutePath(string path)
            // check if we're on a single root filesystem and it's an absolute path
            if (LocationGlobber.IsSingleFileSystemAbsolutePath(path))
            return path.Contains(':');
        /// Determines if the specified path is a root of a UNC share
        /// by counting the path separators "\" following "\\". If only
        /// one path separator is found we know the path is in the form
        /// "\\server\share" and is a valid UNC root.
        /// The path to check to see if its a UNC root.
        /// True if the path is a UNC root, or false otherwise.
        private static bool IsUNCRoot(string path)
                    int lastIndex = path.Length - 1;
                    if (path[path.Length - 1] == '\\')
                        lastIndex--;
                    int separatorsFound = 0;
                        lastIndex = path.LastIndexOf('\\', lastIndex);
                        if (lastIndex == -1)
                        --lastIndex;
                        if (lastIndex < 3)
                        ++separatorsFound;
                    } while (lastIndex > 3);
                    if (separatorsFound == 1)
        /// Determines if the specified path is either a drive root or a UNC root.
        /// The path
        /// True if the path is either a drive root or a UNC root, or false otherwise.
        private static bool IsPathRoot(string path)
            bool isDriveRoot = string.Equals(path, Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase);
            bool isUNCRoot = IsUNCRoot(path);
            bool result = isDriveRoot || isUNCRoot;
            s_tracer.WriteLine("result = {0}; isDriveRoot = {1}; isUNCRoot = {2}", result, isDriveRoot, isUNCRoot);
        /// Normalizes the path that was passed in and returns it as a normalized
        /// path relative to the given basePath.
        /// A fully qualifiedpath to an item. The item must exist,
        /// or the provider writes out an error.
        /// The path that the normalized path should be relative to.
        /// A normalized path, relative to the given basePath.
        protected override string NormalizeRelativePath(
            string basePath)
            if (string.IsNullOrEmpty(path) || !IsValidPath(path))
            basePath ??= string.Empty;
            s_tracer.WriteLine("basePath = {0}", basePath);
                // If it's not fully normalized, normalize it.
                path = NormalizeRelativePathHelper(path, basePath);
                basePath = NormalizePath(basePath);
                basePath = EnsureDriveIsRooted(basePath);
                    string originalPathComparison = path;
                    if (!originalPathComparison.EndsWith(StringLiterals.DefaultPathSeparator))
                        originalPathComparison += StringLiterals.DefaultPathSeparator;
                    string basePathComparison = basePath;
                    if (!basePathComparison.EndsWith(StringLiterals.DefaultPathSeparator))
                        basePathComparison += StringLiterals.DefaultPathSeparator;
                    if (originalPathComparison.StartsWith(basePathComparison, StringComparison.OrdinalIgnoreCase))
                        if (!Utils.PathIsUnc(result))
                            // Add the base path back on so that it can be used for
                            // processing
                            if (!result.StartsWith(basePath, StringComparison.Ordinal))
                                result = MakePath(basePath, result);
                        if (IsPathRoot(result))
                            result = EnsureDriveIsRooted(result);
                            // Now ensure that we have the proper casing by
                            // getting the names of the files and directories that match
                            string directoryPath = GetParentPath(result, string.Empty);
                            if (string.IsNullOrEmpty(directoryPath))
                            // We don't use the Directory.EnumerateFiles() for Unix because the path
                            // may contain additional globbing patterns such as '[ab]'
                            // which Directory.EnumerateFiles() processes, giving undesirable
                            // results in this context.
                            if (!File.Exists(result) && !Directory.Exists(result))
                            string leafName = GetChildName(result);
                            // Use the Directory class to get the real path (this will
                            // ensure the proper casing
                            IEnumerable<string> files = Directory.EnumerateFiles(directoryPath, leafName);
                            if (files == null || !files.Any())
                                files = Directory.EnumerateDirectories(directoryPath, leafName);
                            result = files.First();
                            if (result.StartsWith(basePath, StringComparison.Ordinal))
                                result = result.Substring(basePath.Length);
                                string error = StringUtil.Format(FileSystemProviderStrings.PathOutSideBasePath, path);
                                    new ArgumentException(error);
                                    "PathOutSideBasePath",
                    WriteError(new ErrorRecord(argumentException, "NormalizeRelativePathArgumentError", ErrorCategory.InvalidArgument, path));
                catch (DirectoryNotFoundException directoryNotFound)
                    WriteError(new ErrorRecord(directoryNotFound, "NormalizeRelativePathDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
                    WriteError(new ErrorRecord(ioError, "NormalizeRelativePathIOError", ErrorCategory.ReadError, path));
                    WriteError(new ErrorRecord(accessException, "NormalizeRelativePathUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        /// A fully qualified provider specific path to an item. The item should exist
        /// A normalized path that is relative to the basePath that was passed. The
        /// provider should parse the path parameter, normalize the path, and then
        /// return the normalized path relative to the basePath.
        /// This method does not have to be purely syntactical parsing of the path. It
        /// is encouraged that the provider actually use the path to lookup in its store
        /// and create a relative path that matches the casing, and standardized path syntax.
        /// Note, the base class implementation uses GetParentPath, GetChildName, and MakePath
        /// to normalize the path and then make it relative to basePath. All string comparisons
        /// are done using StringComparison.InvariantCultureIgnoreCase.
        private string NormalizeRelativePathHelper(string path, string basePath)
            string alternateDataStream = string.Empty;
                string newPath = path.Substring(0, secondColon);
                alternateDataStream = path.Replace(newPath, string.Empty);
                path = newPath;
                // Convert to the correct path separators and trim trailing separators
                basePath = basePath.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
                basePath = basePath.TrimEnd(StringLiterals.DefaultPathSeparator);
                path = RemoveRelativeTokens(path);
                // See if the base and the path are already the same. We resolve this to
                // ..\Leaf, since resolving "." to "." doesn't offer much information.
                if (string.Equals(path, basePath, StringComparison.OrdinalIgnoreCase) &&
                    (!originalPath.EndsWith(StringLiterals.DefaultPathSeparator)))
                    string childName = GetChildName(path);
                    result = MakePath("..", childName);
                Stack<string> tokenizedPathStack = null;
                // If the base path isn't really a base, then we resolve to a parent
                // path (such as ../../foo)
                // For example: base = c:/temp/bar/baz
                //              path = c:/temp/foo
                    !(path + StringLiterals.DefaultPathSeparator).StartsWith(
                    basePath + StringLiterals.DefaultPathSeparator, StringComparison.OrdinalIgnoreCase)) &&
                    (!string.IsNullOrEmpty(basePath))
                    string commonBase = GetCommonBase(path, basePath);
                    Stack<string> parentNavigationStack = TokenizePathToStack(basePath, commonBase);
                    int parentPopCount = parentNavigationStack.Count;
                    if (string.IsNullOrEmpty(commonBase))
                        parentPopCount--;
                    for (int leafCounter = 0; leafCounter < parentPopCount; leafCounter++)
                        result = MakePath("..", result);
                    // This is true if we get passed a base path like:
                    //    c:\directory1\directory2
                    // and an actual path of
                    //    c:\directory1
                    // Which happens when the user is in c:\directory1\directory2
                    // and wants to resolve something like:
                    // ..\..\dir*
                    // In that case (as above,) we keep the ..\..\directory1
                    // instead of ".." as would usually be returned
                    if (!string.IsNullOrEmpty(commonBase))
                        if (string.Equals(path, commonBase, StringComparison.OrdinalIgnoreCase) &&
                            (!path.EndsWith(StringLiterals.DefaultPathSeparator)))
                            result = MakePath(result, childName);
                            string[] childNavigationItems = TokenizePathToStack(path, commonBase).ToArray();
                            for (int leafCounter = 0; leafCounter < childNavigationItems.Length; leafCounter++)
                                result = MakePath(result, childNavigationItems[leafCounter]);
                // Otherwise, we resolve to a child path (such as foo/bar)
                    // If the path is a root, then the result will either be empty or the root depending
                    // on the value of basePath.
                    if (IsPathRoot(path))
                    tokenizedPathStack = TokenizePathToStack(path, basePath);
                    // Now we have to normalize the path
                    // by processing each token on the stack
                    Stack<string> normalizedPathStack;
                        normalizedPathStack = NormalizeThePath(basePath, tokenizedPathStack);
                        WriteError(new ErrorRecord(argumentException, "NormalizeRelativePathHelperArgumentError", ErrorCategory.InvalidArgument, null));
                    // Now that the path has been normalized, create the relative path
                    result = CreateNormalizedRelativePathFromStack(normalizedPathStack);
            if (!string.IsNullOrEmpty(alternateDataStream))
                result += alternateDataStream;
        private string RemoveRelativeTokens(string path)
            string testPath = path.Replace('/', '\\');
                !testPath.Contains('\\') ||
                testPath.StartsWith(".\\", StringComparison.Ordinal) ||
                testPath.StartsWith("..\\", StringComparison.Ordinal) ||
                testPath.EndsWith("\\.", StringComparison.Ordinal) ||
                testPath.EndsWith("\\..", StringComparison.Ordinal) ||
                testPath.Contains("\\.\\", StringComparison.Ordinal) ||
                testPath.Contains("\\..\\", StringComparison.Ordinal))
                    Stack<string> tokenizedPathStack = TokenizePathToStack(path, string.Empty);
                    Stack<string> normalizedPath = NormalizeThePath(string.Empty, tokenizedPathStack);
                    return CreateNormalizedRelativePathFromStack(normalizedPath);
                    // Catch any errors here, as we may be in an AppContainer
        /// Get the common base path of two paths.
        /// <param name="path1">One path.</param>
        /// <param name="path2">Another path.</param>
        private string GetCommonBase(string path1, string path2)
            // Always see if the shorter path is a substring of the
            // longer path. If it is not, take the child off of the longer
            // path and compare again.
            while (!string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase))
                if (path2.Length > path1.Length)
                    path2 = GetParentPath(path2, null);
                    path1 = GetParentPath(path1, null);
            return path1;
        /// Tokenizes the specified path onto a stack.
        /// The path to tokenize.
        /// The base part of the path that should not be tokenized.
        /// A stack containing the tokenized path with leaf elements on the bottom
        /// of the stack and the most ancestral parent at the top.
        private Stack<string> TokenizePathToStack(string path, string basePath)
            Stack<string> tokenizedPathStack = new Stack<string>();
            string tempPath = path;
            while (tempPath.Length > basePath.Length)
                // Get the child name and push it onto the stack
                // if its valid
                string childName = GetChildName(tempPath);
                if (string.IsNullOrEmpty(childName))
                    // Push the parent on and then stop
                    s_tracer.WriteLine("tokenizedPathStack.Push({0})", tempPath);
                    tokenizedPathStack.Push(tempPath);
                s_tracer.WriteLine("tokenizedPathStack.Push({0})", childName);
                tokenizedPathStack.Push(childName);
                // Get the parent path and verify if we have to continue
                // tokenizing
                // We are done if the remaining path is:
                // - the same as the previous path
                // - a UNC path that is the root of a UNC share
                // - not a UNC path and the string length is less than or
                //   equal to 3. "C:\"
                tempPath = GetParentPath(tempPath, basePath);
                if (tempPath.Length >= previousParent.Length ||
                    IsPathRoot(tempPath))
                previousParent = tempPath;
            return tokenizedPathStack;
        /// Given the tokenized path, the relative path elements are removed.
        /// <param name="basepath">
        ///   String containing basepath for which we are trying to find the relative path.
        /// <param name="tokenizedPathStack">
        /// A stack containing path elements where the leaf most element is at
        /// the bottom of the stack and the most ancestral parent is on the top.
        /// Generally this stack comes from TokenizePathToStack().
        /// A stack in reverse order with the path elements normalized and all relative
        /// path tokens removed.
        private Stack<string> NormalizeThePath(string basepath, Stack<string> tokenizedPathStack)
            Stack<string> normalizedPathStack = new Stack<string>();
            string currentPath = basepath;
            while (tokenizedPathStack.Count > 0)
                string childName = tokenizedPathStack.Pop();
                s_tracer.WriteLine("childName = {0}", childName);
                // Ignore the current directory token
                if (childName.Equals(".", StringComparison.OrdinalIgnoreCase))
                    // Just ignore it and move on.
                else if (childName.Equals("..", StringComparison.OrdinalIgnoreCase))
                    if (normalizedPathStack.Count > 0)
                        // Pop the result and continue processing
                        string poppedName = normalizedPathStack.Pop();
                        // update our currentpath to reflect the change.
                        if (currentPath.Length > poppedName.Length)
                            currentPath = currentPath.Substring(0, currentPath.Length - poppedName.Length - 1);
                            currentPath = string.Empty;
                        s_tracer.WriteLine("normalizedPathStack.Pop() : {0}", poppedName);
                        throw PSTraceSource.NewArgumentException("path", FileSystemProviderStrings.PathOutSideBasePath);
                    currentPath = MakePath(currentPath, childName);
                    var fsinfo = GetFileSystemInfo(currentPath, out bool _);
                    // Clean up the child name to proper casing and short-path
                    // expansion if required. Also verify that .NET hasn't over-normalized
                    // the path
                        // This might happen if you've passed a child name of two or more dots,
                        // which the .NET APIs treat as the parent directory
                        if (fsinfo.FullName.Length < currentPath.Length)
                            throw PSTraceSource.NewArgumentException("path", FileSystemProviderStrings.ItemDoesNotExist, currentPath);
                        // Expand the short file name
                        if (fsinfo.Name.Length >= childName.Length)
                            childName = fsinfo.Name;
                        // We couldn't find the item
                        if (tokenizedPathStack.Count == 0)
                s_tracer.WriteLine("normalizedPathStack.Push({0})", childName);
                normalizedPathStack.Push(childName);
            return normalizedPathStack;
        /// Pops each leaf element of the stack and uses MakePath to generate the relative path.
        /// <param name="normalizedPathStack">
        /// The stack containing the leaf elements of the path.
        /// A path that is made up of the leaf elements on the given stack.
        /// The elements on the stack start from the leaf element followed by its parent
        /// followed by its parent, etc. Each following element on the stack is the parent
        /// of the one before it.
        private string CreateNormalizedRelativePathFromStack(Stack<string> normalizedPathStack)
            string leafElement = string.Empty;
            while (normalizedPathStack.Count > 0)
                if (string.IsNullOrEmpty(leafElement))
                    leafElement = normalizedPathStack.Pop();
                    string parentElement = normalizedPathStack.Pop();
                    leafElement = MakePath(parentElement, leafElement);
            return leafElement;
                // Since there was no path separator return an empty string
                result = EnsureDriveIsRooted(path);
        /// Determines if the item at the specified path is a directory.
        /// The path to the file or directory to check.
        /// True if the item at the specified path is a directory.
            return Directory.Exists(path);
            if (string.IsNullOrEmpty(destination))
                throw PSTraceSource.NewArgumentException(nameof(destination));
            if (PathIsReservedDeviceName(destination, "MoveError"))
                s_tracer.WriteLine("Moving {0} to {1}", path, destination);
                    if (ItemExists(destination) &&
                        IsItemContainer(destination))
                        destination = MakePath(destination, dir.Name);
                    // Don't allow moving a directory into itself or its sub-directory.
                    string pathWithoutEndingSeparator = Path.TrimEndingDirectorySeparator(path);
                    if (destination.StartsWith(pathWithoutEndingSeparator + Path.DirectorySeparatorChar)
                        || destination.Equals(pathWithoutEndingSeparator, StringComparison.OrdinalIgnoreCase))
                        string error = StringUtil.Format(FileSystemProviderStrings.TargetCannotBeSubdirectoryOfSource, destination);
                        var e = new IOException(error);
                        WriteError(new ErrorRecord(e, "MoveItemArgumentError", ErrorCategory.InvalidArgument, destination));
                    // Get the confirmation text
                    string action = FileSystemProviderStrings.MoveItemActionDirectory;
                    string resource = StringUtil.Format(FileSystemProviderStrings.MoveItemResourceFileTemplate, dir.FullName, destination);
                    // Confirm the move with the user
                        // Now move the directory
                        MoveDirectoryInfoItem(dir, destination, Force);
                        "FileInfo should be throwing an exception but it's " +
                        "returning null instead");
                        // Construct the new file path from the destination
                        // directory and the file name
                        destination = MakePath(destination, file.Name);
                    string action = FileSystemProviderStrings.MoveItemActionFile;
                    string resource = StringUtil.Format(FileSystemProviderStrings.MoveItemResourceFileTemplate, file.FullName, destination);
                        MoveFileInfoItem(file, destination, Force, true);
                WriteError(new ErrorRecord(argException, "MoveItemArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "MoveItemIOError", ErrorCategory.WriteError, path));
                WriteError(new ErrorRecord(accessException, "MoveItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        private void MoveFileInfoItem(
            FileInfo file,
            bool output)
                !string.IsNullOrEmpty(destination),
                "THe caller should verify destination.");
                // Move the file
                file.MoveTo(destination);
                if (output)
            catch (System.UnauthorizedAccessException unauthorizedAccess)
                // This error is thrown when the readonly bit is set.
                        // mask off the readonly and hidden bits and try again
                        file.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
                            WriteItemObject(file, file.FullName, false);
                            (e is ArgumentNullException) ||
                            (e is FileNotFoundException) ||
                            (e is DirectoryNotFoundException) ||
                            // If any exception occurs return the original error
                            WriteError(new ErrorRecord(unauthorizedAccess, "MoveFileInfoItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, file));
                WriteError(new ErrorRecord(argException, "MoveFileInfoItemArgumentError", ErrorCategory.InvalidArgument, file));
                // check if destination file exists. if force is specified then we should delete the destination before moving
                if (force && File.Exists(destination))
                    FileInfo destfile = new FileInfo(destination);
                                WriteError(new ErrorRecord(ioException, "MoveFileInfoItemIOError", ErrorCategory.WriteError, destfile));
                        WriteError(new ErrorRecord(ioException, "MoveFileInfoItemIOError", ErrorCategory.WriteError, file));
        private void MoveDirectoryInfoItem(
                "The caller should verify destination.");
                MoveDirectoryInfoUnchecked(directory, destination, force);
                    directory,
                    directory.FullName,
                        directory.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
                        WriteItemObject(directory, directory.FullName, true);
                        WriteError(new ErrorRecord(unauthorizedAccess, "MoveDirectoryItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, directory));
                            (exception is ArgumentException))
                WriteError(new ErrorRecord(argException, "MoveDirectoryItemArgumentError", ErrorCategory.InvalidArgument, directory));
                WriteError(new ErrorRecord(ioException, "MoveDirectoryItemIOError", ErrorCategory.WriteError, directory));
        /// Implements the file move operation for directories without handling any error scenarios.
        /// In particular, this attempts to rename or copy+delete the file,
        /// but passes any exceptional behavior through to the caller.
        /// <param name="directory">The directory to move.</param>
        /// <param name="destinationPath">The destination path to move the directory to.</param>
        /// <param name="force">If true, force move the directory, overwriting anything at the destination.</param>
        private void MoveDirectoryInfoUnchecked(DirectoryInfo directory, string destinationPath, bool force)
                if (InternalTestHooks.ThrowExdevErrorOnMoveDirectory)
                    throw new IOException("Invalid cross-device link");
                directory.MoveTo(destinationPath);
            // This is the errno returned by the rename() syscall
            // when an item is attempted to be renamed across filesystem mount boundaries.
            // 0x80131620 is returned if the source and destination do not have the same root path
            catch (IOException e) when (e.HResult == 18 || e.HResult == -2146232800)
            // 0x80070005 ACCESS_DENIED is returned when trying to move files across volumes like DFS
            catch (IOException e) when (e.HResult == -2147024891 || e.HResult == -2146232800)
                // Rather than try to ascertain whether we can rename a directory ahead of time,
                // it's both faster and more correct to try to rename it and fall back to copy/deleting it
                // See also: https://github.com/coreutils/coreutils/blob/439741053256618eb651e6d43919df29625b8714/src/mv.c#L212-L216
                CopyAndDelete(directory, destinationPath, force);
        private void CopyAndDelete(DirectoryInfo directory, string destination, bool force)
            if (!ItemExists(destination))
            else if (ItemExists(destination) && !IsItemContainer(destination))
                string errorMessage = StringUtil.Format(FileSystemProviderStrings.DirectoryExist, destination);
            foreach (FileInfo file in directory.EnumerateFiles())
                MoveFileInfoItem(file, Path.Combine(destination, file.Name), force, false);
            foreach (DirectoryInfo dir in directory.EnumerateDirectories())
                CopyAndDelete(dir, Path.Combine(destination, dir.Name), force);
            if (!directory.EnumerateDirectories().Any() && !directory.EnumerateFiles().Any())
                RemoveItem(directory.FullName, false);
        #endregion NavigationCmdletProvider members
        #region IPropertyCmdletProvider
        /// Gets a property for the given item.
        /// <param name="path">The fully qualified path to the item.</param>
        /// The list of properties to get.  Examples include "Attributes", "LastAccessTime,"
        /// and other properties defined by
        /// <see cref="System.IO.DirectoryInfo"/> and
        /// <see cref="System.IO.FileInfo"/>
        public void GetProperty(string path, Collection<string> providerSpecificPickList)
            PSObject result = null;
                var fileSystemObject = GetFileSystemInfo(path, out bool isDirectory);
                if (fileSystemObject == null)
                    // Finally get the properties
                    if (providerSpecificPickList == null || providerSpecificPickList.Count == 0)
                        result = PSObject.AsPSObject(fileSystemObject);
                        foreach (string property in providerSpecificPickList)
                            if (property != null && property.Length > 0)
                                    PSObject mshObject = PSObject.AsPSObject(fileSystemObject);
                                    PSMemberInfo member = mshObject.Properties[property];
                                        value = member.Value;
                                        result ??= new PSObject();
                                        result.Properties.Add(new PSNoteProperty(property, value));
                                        string error =
                                                FileSystemProviderStrings.PropertyNotFound,
                                                property);
                                        WriteError(new ErrorRecord(e, "GetValueError", ErrorCategory.ReadError, property));
                                catch (GetValueException exception)
                                    WriteError(new ErrorRecord(exception, "GetValueError", ErrorCategory.ReadError, property));
                WriteError(new ErrorRecord(argException, "GetPropertyArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "GetPropertyIOError", ErrorCategory.ReadError, path));
                WriteError(new ErrorRecord(accessException, "GetPropertyUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
                WritePropertyObject(result, path);
        /// Gets the dynamic property parameters required by the get-itemproperty cmdlet.
        /// This feature is not required by the File System provider.
        /// A list of properties that should be retrieved. If this parameter is null
        /// or empty, all properties should be retrieved.
        /// Null.  This feature is not required by the File System provider.
        public object GetPropertyDynamicParameters(
        /// Sets the specified properties on the item at the given path.
        /// The path of the item on which to set the properties.
        /// <param name="propertyToSet">
        /// A PSObject which contains a collection of the names and values
        /// of the properties to be set.  The File System provider supports setting
        /// only the "Attributes" property.
        ///     propertyToSet is null.
        public void SetProperty(string path, PSObject propertyToSet)
            if (propertyToSet == null)
                throw PSTraceSource.NewArgumentNullException(nameof(propertyToSet));
            PSObject results = new PSObject();
            // Create a PSObject with either a DirectoryInfo or FileInfo object at its core.
                PSObject fileSystemInfoShell = PSObject.AsPSObject(fsinfo);
                bool propertySet = false;
                foreach (PSMemberInfo property in propertyToSet.Properties)
                        action = FileSystemProviderStrings.SetPropertyActionDirectory;
                        action = FileSystemProviderStrings.SetPropertyActionFile;
                    string resourceTemplate = FileSystemProviderStrings.SetPropertyResourceTemplate;
                    string propertyValueString;
                        // Use a PSObject to get the string representation of the property value
                        PSObject propertyValuePSObject = PSObject.AsPSObject(propertyValue);
                        propertyValueString = propertyValuePSObject.ToString();
                            "FileSystemProvider.SetProperty exception " + e.Message);
                            Host.CurrentCulture,
                            propertyValueString);
                    // Confirm the set with the user
                        PSObject mshObject = PSObject.AsPSObject(fileSystemInfoShell);
                        PSMemberInfo member = mshObject.Properties[property.Name];
                            if (string.Equals(property.Name, "attributes", StringComparison.OrdinalIgnoreCase))
                                FileAttributes attributes;
                                if (propertyValue is FileAttributes)
                                    attributes = (FileAttributes)propertyValue;
                                    attributes = (FileAttributes)Enum.Parse(typeof(FileAttributes), propertyValueString, true);
                                if ((attributes & ~(FileAttributes.Archive | FileAttributes.Hidden |
                                                        FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.System)) != 0)
                                            FileSystemProviderStrings.AttributesNotSupported,
                                    WriteError(new ErrorRecord(e, "SetPropertyError", ErrorCategory.ReadError, property));
                            member.Value = propertyValue;
                            results.Properties.Add(new PSNoteProperty(property.Name, propertyValue));
                            propertySet = true;
                if (propertySet)
                    WritePropertyObject(results, path);
        /// Gets the dynamic property parameters required by the set-itemproperty cmdlet.
        /// to the item for which to set the dynamic parameters.
        /// A PSObject which contains a collection of the name, type, value
        /// of the properties to be set.
        public object SetPropertyDynamicParameters(
        /// Clears the specified properties on the item at the given path.
        /// The File System provider supports only the "Attributes" property.
        /// The path of the item on which to clear the properties.
        /// <param name="propertiesToClear">
        /// A collection of the names of the properties to clear.  The File System
        /// provider supports clearing only the "Attributes" property.
        ///     Path is null or empty.
        ///     propertiesToClear is null or count is zero.
        public void ClearProperty(
            Collection<string> propertiesToClear)
            if (propertiesToClear == null ||
                propertiesToClear.Count == 0)
                throw PSTraceSource.NewArgumentNullException(nameof(propertiesToClear));
            // Only the attributes property can be cleared
            if (propertiesToClear.Count > 1 ||
                Host.CurrentCulture.CompareInfo.Compare("Attributes", propertiesToClear[0], CompareOptions.IgnoreCase) != 0)
                throw PSTraceSource.NewArgumentException(nameof(propertiesToClear), FileSystemProviderStrings.CannotClearProperty);
                // Now the only entry in the array should be the Attributes, so clear them
                FileSystemInfo fileSystemInfo = null;
                    action = FileSystemProviderStrings.ClearPropertyActionDirectory;
                    fileSystemInfo = new DirectoryInfo(path);
                    action = FileSystemProviderStrings.ClearPropertyActionFile;
                    fileSystemInfo = new FileInfo(path);
                string resourceTemplate = FileSystemProviderStrings.ClearPropertyResourceTemplate;
                        propertiesToClear[0]);
                    fileSystemInfo.Attributes = FileAttributes.Normal;
                    result.Properties.Add(new PSNoteProperty(propertiesToClear[0], fileSystemInfo.Attributes));
                    // Now write out the attribute that was cleared.
                WriteError(new ErrorRecord(unauthorizedAccessException, "ClearPropertyUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
                WriteError(new ErrorRecord(argException, "ClearPropertyArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "ClearPropertyIOError", ErrorCategory.WriteError, path));
        /// Gets the dynamic property parameters required by the clear-itemproperty cmdlet.
        /// A collection of the names of the properties to clear.
        public object ClearPropertyDynamicParameters(
        #endregion IPropertyCmdletProvider
        #region IContentCmdletProvider
        /// Creates an instance of the FileSystemContentStream class, opens
        /// the specified file for reading, and returns the IContentReader interface
        /// to it.
        /// The path of the file to be opened for reading.
        /// An IContentReader for the specified file.
        public IContentReader GetContentReader(string path)
            // Defaults for the file read operation
            string delimiter = "\n";
            Encoding encoding = Encoding.Default;
            bool waitForChanges = false;
            bool streamTypeSpecified = false;
            bool usingByteEncoding = false;
            bool delimiterSpecified = false;
            bool isRawStream = false;
            string streamName = null;
            // Get the dynamic parameters.
            // They override the defaults specified above.
                FileSystemContentReaderDynamicParameters dynParams =
                    DynamicParameters as FileSystemContentReaderDynamicParameters;
                if (dynParams != null)
                    // -raw is not allowed when -first,-last or -wait is specified
                    // this call will validate that and throws.
                    ValidateParameters(dynParams.Raw);
                    isRawStream = dynParams.Raw;
                    // Get the delimiter
                    delimiterSpecified = dynParams.DelimiterSpecified;
                    if (delimiterSpecified)
                        delimiter = dynParams.Delimiter;
                    // Get the stream type
                    usingByteEncoding = dynParams.AsByteStream;
                    streamTypeSpecified = dynParams.WasStreamTypeSpecified;
                    if (usingByteEncoding && streamTypeSpecified)
                        WriteWarning(FileSystemProviderStrings.EncodingNotUsed);
                    if (streamTypeSpecified)
                        encoding = dynParams.Encoding;
                    // Get the wait value
                    waitForChanges = dynParams.Wait;
                    // Get the stream name
                    streamName = dynParams.Stream;
                streamName = path.Substring(secondColon + 1);
            FileSystemContentReaderWriter stream = null;
                // Get-Content will write a non-terminating error if the target is a directory.
                // On Windows, the streamName must be null or empty for it to write the error. Otherwise, the
                // alternate data stream is not a directory, even if it's set on a directory.
                if (Directory.Exists(path)
                    && string.IsNullOrEmpty(streamName)
                    string errMsg = StringUtil.Format(SessionStateStrings.GetContainerContentException, path);
                    ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), "GetContainerContentException", ErrorCategory.InvalidOperation, null);
                    return stream;
                // Users can't both read as bytes, and specify a delimiter
                    if (usingByteEncoding)
                            new ArgumentException(FileSystemProviderStrings.DelimiterError, "delimiter");
                            "GetContentReaderArgumentError",
                        // Initialize the file reader
                        stream = new FileSystemContentReaderWriter(path, streamName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, delimiter, encoding, waitForChanges, this, isRawStream);
                    stream = new FileSystemContentReaderWriter(path, streamName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, encoding, usingByteEncoding, waitForChanges, this, isRawStream);
                WriteError(new ErrorRecord(pathTooLong, "GetContentReaderPathTooLongError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(fileNotFound, "GetContentReaderFileNotFoundError", ErrorCategory.ObjectNotFound, path));
                WriteError(new ErrorRecord(directoryNotFound, "GetContentReaderDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
                WriteError(new ErrorRecord(argException, "GetContentReaderArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "GetContentReaderIOError", ErrorCategory.ReadError, path));
                WriteError(new ErrorRecord(securityException, "GetContentReaderSecurityError", ErrorCategory.PermissionDenied, path));
            catch (UnauthorizedAccessException unauthorizedAccess)
                WriteError(new ErrorRecord(unauthorizedAccess, "GetContentReaderUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        /// Gets the dynamic property parameters required by the get-content cmdlet.
        public object GetContentReaderDynamicParameters(string path)
            return new FileSystemContentReaderDynamicParameters(this);
        /// the specified file for writing, and returns the IContentReader interface
        /// The path of the file to be opened for writing.
        /// An IContentWriter for the specified file.
        public IContentWriter GetContentWriter(string path)
            // If this is true, then the content will be read as bytes
            const FileMode filemode = FileMode.OpenOrCreate;
            bool suppressNewline = false;
            // Get the dynamic parameters
                FileSystemContentWriterDynamicParameters dynParams =
                    DynamicParameters as FileSystemContentWriterDynamicParameters;
                    suppressNewline = dynParams.NoNewline.IsPresent;
                // Add-Content and Set-Content will write a non-terminating error if the target is a directory.
                    string errMsg = StringUtil.Format(SessionStateStrings.WriteContainerContentException, path);
                    ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), "WriteContainerContentException", ErrorCategory.InvalidOperation, null);
                stream = new FileSystemContentReaderWriter(path, streamName, filemode, FileAccess.Write, FileShare.ReadWrite, encoding, usingByteEncoding, false, this, false, suppressNewline);
                WriteError(new ErrorRecord(pathTooLong, "GetContentWriterPathTooLongError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(fileNotFound, "GetContentWriterFileNotFoundError", ErrorCategory.ObjectNotFound, path));
                WriteError(new ErrorRecord(directoryNotFound, "GetContentWriterDirectoryNotFoundError", ErrorCategory.ObjectNotFound, path));
                WriteError(new ErrorRecord(argException, "GetContentWriterArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "GetContentWriterIOError", ErrorCategory.WriteError, path));
                WriteError(new ErrorRecord(securityException, "GetContentWriterSecurityError", ErrorCategory.PermissionDenied, path));
                WriteError(new ErrorRecord(unauthorizedAccess, "GetContentWriterUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        /// Gets the dynamic property parameters required by the set-content and
        /// add-content cmdlets.
        public object GetContentWriterDynamicParameters(string path)
            return new FileSystemContentWriterDynamicParameters(this);
        /// Clears the content of the specified file.
        /// The path to the file of which to clear the contents.
        public void ClearContent(string path)
                bool clearStream = false;
                FileSystemClearContentDynamicParameters dynamicParameters = null;
                FileSystemContentWriterDynamicParameters writerDynamicParameters = null;
                // We get called during:
                //     - Clear-Content
                //     - Set-Content, in the phase that clears the path first.
                    dynamicParameters = DynamicParameters as FileSystemClearContentDynamicParameters;
                    writerDynamicParameters = DynamicParameters as FileSystemContentWriterDynamicParameters;
                            clearStream = true;
                        streamName = dynamicParameters.Stream;
                    else if (writerDynamicParameters != null)
                        if ((writerDynamicParameters.Stream != null) && (writerDynamicParameters.Stream.Length > 0))
                        streamName = writerDynamicParameters.Stream;
                    if (string.IsNullOrEmpty(streamName))
                // If they're just working on the DATA stream, don't use the Alternate Data Stream
                // utils to clear the stream - otherwise, the Win32 API will trash the other streams.
                if (string.Equals(":$DATA", streamName, StringComparison.OrdinalIgnoreCase))
                    clearStream = false;
                // On Windows, determine if our argument is a directory only after we determine if
                // we're being asked to work with an alternate data stream, because directories can have
                // alternate data streams on them that are not child items. These alternate data streams
                // must be treated as data streams, even if they're attached to directories. However,
                // if asked to work with a directory without a data stream specified, write a non-terminating
                // error instead of clearing all child items of the directory. (On non-Windows, alternate
                // data streams don't exist, so in that environment always write the error when addressing
                // a directory.)
                    && !clearStream
                    string errorMsg = StringUtil.Format(SessionStateStrings.ClearDirectoryContent, path);
                    WriteError(new ErrorRecord(new NotSupportedException(errorMsg), "ClearDirectoryContent", ErrorCategory.InvalidOperation, path));
                if (clearStream)
                    FileStream fileStream = null;
                    string streamAction = string.Format(
                        streamName, path);
                    if (ShouldProcess(streamAction))
                        // If we've been called as part of Clear-Content, validate that the stream exists.
                        // This is because the core API doesn't support truncate mode.
                            fileStream = AlternateDataStreamUtilities.CreateFileStream(path, streamName, FileMode.Open, FileAccess.Write, FileShare.Write);
                        fileStream = AlternateDataStreamUtilities.CreateFileStream(
                            path, streamName, FileMode.Create, FileAccess.Write, FileShare.Write);
                    string action = FileSystemProviderStrings.ClearContentActionFile;
                    string resource = StringUtil.Format(FileSystemProviderStrings.ClearContentesourceTemplate, path);
                    if (!ShouldProcess(resource, action))
                    FileStream fileStream = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Write);
                // For filesystem once content is cleared
                WriteItemObject(string.Empty, path, false);
                WriteError(new ErrorRecord(argException, "ClearContentArgumentError", ErrorCategory.InvalidArgument, path));
                WriteError(new ErrorRecord(ioException, "ClearContentIOError", ErrorCategory.WriteError, path));
                    //// Store the old attributes so that we can recover them
                    FileAttributes oldAttributes = File.GetAttributes(path);
                        // Since a security exception was thrown, try to mask off
                        // the hidden and readonly bits and then retry.
                        File.SetAttributes(path, (File.GetAttributes(path) & ~(FileAttributes.Hidden | FileAttributes.ReadOnly)));
                    catch (UnauthorizedAccessException failure)
                        WriteError(new ErrorRecord(failure, "RemoveFileSystemItemUnAuthorizedAccess", ErrorCategory.PermissionDenied, path));
                        //// Reset the attributes
                        File.SetAttributes(path, oldAttributes);
                    WriteError(new ErrorRecord(accessException, "ClearContentUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
        /// Gets the dynamic property parameters required by the clear-content cmdlet.
        /// A FileSystemClearContentDynamicParameters that provides access to the -Stream dynamic parameter.
        public object ClearContentDynamicParameters(string path)
            return new FileSystemClearContentDynamicParameters();
        #endregion IContentCmdletProvider
        /// -raw is not allowed when -first,-last or -wait is specified
        /// this call will validate that and throws.
        private void ValidateParameters(bool isRawSpecified)
            if (isRawSpecified)
                if (this.Context.MyInvocation.BoundParameters.ContainsKey("TotalCount"))
                    string message = StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "TotalCount");
                if (this.Context.MyInvocation.BoundParameters.ContainsKey("Tail"))
                    string message = StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Tail");
                if (this.Context.MyInvocation.BoundParameters.ContainsKey("Wait"))
                    string message = StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Wait");
                if (this.Context.MyInvocation.BoundParameters.ContainsKey("Delimiter"))
                    string message = StringUtil.Format(FileSystemProviderStrings.NoFirstLastWaitForRaw, "Raw", "Delimiter");
        /// The API 'PathIsNetworkPath' is not available in CoreSystem.
        /// This implementation is based on the 'PathIsNetworkPath' API.
        internal static bool PathIsNetworkPath(string path)
            return WinPathIsNetworkPath(path);
        /// <param name="path">A file system path.</param>
        /// <returns>True if the path is a network path.</returns>
        internal static bool WinPathIsNetworkPath(string path)
                if (Utils.PathIsUnc(path, networkOnly : true))
                if (path.Length > 1 && path[1] == ':' && char.IsAsciiLetter(path[0]))
                    // path[0] is ASCII letter, e.g. is in 'A'-'Z' or 'a'-'z'.
                    int errorCode = Interop.Windows.GetUNCForNetworkDrive(path[0], out string _);
                    // From the 'IsNetDrive' API.
                    // 0: success; 1201: connection closed; 31: device error
                    if (errorCode == Interop.Windows.ERROR_SUCCESS ||
                        errorCode == Interop.Windows.ERROR_CONNECTION_UNAVAIL ||
                        errorCode == Interop.Windows.ERROR_GEN_FAILURE)
        #region InodeTracker
        /// Tracks visited files/directories by caching their device IDs and inodes.
        private sealed class InodeTracker
            private readonly HashSet<(UInt64, UInt64)> _visitations;
            /// Construct a new InodeTracker with an initial path.
            internal InodeTracker(string path)
                _visitations = new HashSet<(UInt64, UInt64)>();
                if (InternalSymbolicLinkLinkCodeMethods.GetInodeData(path, out (UInt64, UInt64) inodeData))
                    _visitations.Add(inodeData);
            /// Attempt to mark a path as having been visited.
            /// Path to the file system item to be visited.
            /// True if the path had not been previously visited and was
            /// successfully marked as visited, false otherwise.
            internal bool TryVisitPath(string path)
                    returnValue = _visitations.Add(inodeData);
    internal static class SafeInvokeCommand
        public static Hashtable Invoke(System.Management.Automation.PowerShell ps, FileSystemProvider fileSystemContext, CmdletProviderContext cmdletContext)
            return Invoke(ps, fileSystemContext, cmdletContext, true);
        public static Hashtable Invoke(System.Management.Automation.PowerShell ps, FileSystemProvider fileSystemContext, CmdletProviderContext cmdletContext, bool shouldHaveOutput)
            bool useFileSystemProviderContext = (cmdletContext == null);
            if (useFileSystemProviderContext)
                Dbg.Diagnostics.Assert(fileSystemContext != null, "The caller should verify FileSystemProvider context.");
            Collection<Hashtable> output;
                output = ps.Invoke<Hashtable>();
                    fileSystemContext.WriteError(new ErrorRecord(e, "CopyFileRemoteExecutionError", ErrorCategory.InvalidOperation, ps));
                    cmdletContext.WriteError(new ErrorRecord(e, "CopyFileRemoteExecutionError", ErrorCategory.InvalidOperation, ps));
            if (ps.HadErrors)
                foreach (var error in ps.Streams.Error)
                        fileSystemContext.WriteError(error);
                        cmdletContext.WriteError(error);
            if (shouldHaveOutput)
                if (output.Count != 1 || output[0].GetType() != typeof(Hashtable))
                    // unexpected output
                    Dbg.Diagnostics.Assert(output[0] != null, "Expected an output from the remote call.");
                return (Hashtable)output[0];
    internal sealed class CopyItemDynamicParameters
        public PSSession FromSession { get; set; }
        public PSSession ToSession { get; set; }
    /// Defines the container cmdlet dynamic providers.
    internal sealed class GetChildDynamicParameters
        /// Gets or sets the attribute filtering enum evaluator.
        public FlagsExpression<FileAttributes> Attributes { get; set; }
        /// Gets or sets the flag to follow symbolic links when recursing.
        public SwitchParameter FollowSymlink { get; set; }
        /// Gets or sets the filter directory flag.
        [Alias("ad")]
        public SwitchParameter Directory
            get { return _attributeDirectory; }
            set { _attributeDirectory = value; }
        private bool _attributeDirectory;
        /// Gets or sets the filter file flag.
        [Alias("af")]
        public SwitchParameter File
            get { return _attributeFile; }
            set { _attributeFile = value; }
        private bool _attributeFile;
        /// Gets or sets the filter hidden flag.
        [Alias("ah", "h")]
        public SwitchParameter Hidden
            get { return _attributeHidden; }
            set { _attributeHidden = value; }
        private bool _attributeHidden;
        /// Gets or sets the filter readonly flag.
        [Alias("ar")]
        public SwitchParameter ReadOnly
            get { return _attributeReadOnly; }
            set { _attributeReadOnly = value; }
        private bool _attributeReadOnly;
        /// Gets or sets the filter system flag.
        [Alias("as")]
        public SwitchParameter System
            get { return _attributeSystem; }
            set { _attributeSystem = value; }
        private bool _attributeSystem;
    /// Defines the dynamic parameters used by both the content reader and writer.
    public class FileSystemContentDynamicParametersBase
        internal FileSystemContentDynamicParametersBase(FileSystemProvider provider)
        private readonly FileSystemProvider _provider;
        /// Gets or sets the encoding method used when
        /// reading data from the file.
                // Check for UTF-7 by checking for code page 65000
                // See: https://learn.microsoft.com/dotnet/core/compatibility/corefx#utf-7-code-paths-are-obsolete
                if (value != null && value.CodePage == 65000)
                    _provider.WriteWarning(PathUtilsStrings.Utf7EncodingObsolete);
                // If an encoding was explicitly set, be sure to capture that.
                WasStreamTypeSpecified = true;
        /// Return file contents as a byte stream or create file from a series of bytes.
        public SwitchParameter AsByteStream { get; set; }
        /// A parameter to return a stream of an item.
        public string Stream { get; set; }
        /// Gets the status of the StreamType parameter.  Returns true
        /// if the stream was opened with a user-specified encoding, false otherwise.
        public bool WasStreamTypeSpecified { get; private set; }
    /// Defines the dynamic parameters used by the Clear-Content cmdlet.
    public class FileSystemClearContentDynamicParameters
    /// Defines the dynamic parameters used by the set-content and
    public class FileSystemContentWriterDynamicParameters : FileSystemContentDynamicParametersBase
        internal FileSystemContentWriterDynamicParameters(FileSystemProvider provider) : base(provider) { }
    /// Defines the dynamic parameters used by the get-content cmdlet.
    public class FileSystemContentReaderDynamicParameters : FileSystemContentDynamicParametersBase
        internal FileSystemContentReaderDynamicParameters(FileSystemProvider provider) : base(provider) { }
        /// Gets or sets the delimiter to use when reading the file.  Custom delimiters
        /// may not be used when the file is opened with a "Byte" encoding.
                DelimiterSpecified = true;
        private string _delimiter = "\n";
        /// Gets or sets the Wait flag.  The wait flag determines if we want
        /// the read-content call to poll (and wait) for changes to the file,
        /// rather than exit after the content has been read.
        /// When the Raw switch is present, we don't do any breaks on newlines,
        /// and only emit one object to the pipeline: all of the content.
                return _isRaw;
                _isRaw = value;
        private bool _isRaw;
        /// Gets the status of the delimiter parameter.  Returns true
        /// if the delimiter was explicitly specified by the user, false otherwise.
        public bool DelimiterSpecified
            // get
    /// Provides the dynamic parameters for test-path on the file system.
    public class FileSystemItemProviderDynamicParameters
        /// A parameter to test if a file is older than a certain time or date.
        public DateTime? OlderThan { get; set; }
        /// A parameter to test if a file is newer than a certain time or date.
        public DateTime? NewerThan { get; set; }
    /// Provides the dynamic parameters for Get-Item on the file system.
    public class FileSystemProviderGetItemDynamicParameters
        /// A parameter to return the streams of an item.
        public string[] Stream { get; set; }
    /// Provides the dynamic parameters for Remove-Item on the file system.
    public class FileSystemProviderRemoveItemDynamicParameters
    #region Symbolic Link
    /// Class to find the symbolic link target.
    public static partial class InternalSymbolicLinkLinkCodeMethods
        private const int FSCTL_GET_REPARSE_POINT = 0x000900A8;
        private const int FSCTL_SET_REPARSE_POINT = 0x000900A4;
        private const int FSCTL_DELETE_REPARSE_POINT = 0x000900AC;
        private const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
        private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
        private const uint IO_REPARSE_TAG_APPEXECLINK = 0x8000001B;
        private const string NonInterpretedPathPrefix = @"\??\";
        private struct REPARSE_DATA_BUFFER_SYMBOLICLINK
            public uint ReparseTag;
            public ushort ReparseDataLength;
            public ushort Reserved;
            public ushort SubstituteNameOffset;
            public ushort SubstituteNameLength;
            public ushort PrintNameOffset;
            public ushort PrintNameLength;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        private struct REPARSE_DATA_BUFFER_MOUNTPOINT
        private struct BY_HANDLE_FILE_INFORMATION
            public uint FileAttributes;
            public FILE_TIME CreationTime;
            public FILE_TIME LastAccessTime;
            public FILE_TIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        [LibraryImport(PinvokeDllNames.DeviceIoControlDllName, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            IntPtr InBuffer, int nInBufferSize,
            IntPtr OutBuffer, int nOutBufferSize,
            out int pBytesReturned, IntPtr lpOverlapped);
        [LibraryImport(PinvokeDllNames.GetFileInformationByHandleDllName)]
        private static partial bool GetFileInformationByHandle(
                out BY_HANDLE_FILE_INFORMATION lpFileInformation);
        /// Gets the target of the specified reparse point.
        /// <param name="instance">The object of FileInfo or DirectoryInfo type.</param>
        /// <returns>The target of the reparse point.</returns>
        [Obsolete("This method is now obsolete. Please use the .NET API 'FileSystemInfo.LinkTarget'", error: true)]
        public static string GetTarget(PSObject instance)
            if (instance.BaseObject is FileSystemInfo fileSysInfo)
                if (!fileSysInfo.Exists)
                        StringUtil.Format(SessionStateStrings.PathNotFound, fileSysInfo.FullName));
                return fileSysInfo.LinkTarget;
        /// Gets the target for a given file or directory, resolving symbolic links.
        /// <param name="instance">The FileInfo or DirectoryInfo type.</param>
        /// <returns>The file path the instance points to.</returns>
        public static string ResolvedTarget(PSObject instance)
                FileSystemInfo linkTarget = fileSysInfo.ResolveLinkTarget(true);
                return linkTarget is null ? fileSysInfo.FullName : linkTarget.FullName;
        /// Gets the link type of the specified reparse point.
        /// <returns>The link type of the reparse point. SymbolicLink for symbolic links.</returns>
        public static string GetLinkType(PSObject instance)
            FileSystemInfo fileSysInfo = instance.BaseObject as FileSystemInfo;
            if (fileSysInfo != null)
                return InternalGetLinkType(fileSysInfo);
        private static string InternalGetLinkType(FileSystemInfo fileInfo)
            return Platform.NonWindowsInternalGetLinkType(fileInfo);
            return WinInternalGetLinkType(fileInfo.FullName);
        private static string WinInternalGetLinkType(string filePath)
            // We set accessMode parameter to zero because documentation says:
            // If this parameter is zero, the application can query certain metadata
            // such as file, directory, or device attributes without accessing
            // that file or device, even if GENERIC_READ access would have been denied.
            using (SafeFileHandle handle = WinOpenReparsePoint(filePath, (FileAccess)0))
                int outBufferSize = Marshal.SizeOf<REPARSE_DATA_BUFFER_SYMBOLICLINK>();
                IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);
                    int bytesReturned;
                    string linkType = null;
                    // OACR warning 62001 about using DeviceIOControl has been disabled.
                    // According to MSDN guidance DangerousAddRef() and DangerousRelease() have been used.
                    handle.DangerousAddRef(ref success);
                    // Get Buffer size
                    IntPtr dangerousHandle = handle.DangerousGetHandle();
                    bool result = DeviceIoControl(
                        dangerousHandle,
                        FSCTL_GET_REPARSE_POINT,
                        InBuffer: IntPtr.Zero,
                        nInBufferSize: 0,
                        outBuffer,
                        outBufferSize,
                        out bytesReturned,
                        lpOverlapped: IntPtr.Zero);
                        // It's not a reparse point or the file system doesn't support reparse points.
                        return WinIsHardLink(ref dangerousHandle) ? "HardLink" : null;
                    REPARSE_DATA_BUFFER_SYMBOLICLINK reparseDataBuffer = Marshal.PtrToStructure<REPARSE_DATA_BUFFER_SYMBOLICLINK>(outBuffer);
                    switch (reparseDataBuffer.ReparseTag)
                        case IO_REPARSE_TAG_SYMLINK:
                            linkType = "SymbolicLink";
                        case IO_REPARSE_TAG_MOUNT_POINT:
                            linkType = "Junction";
                            linkType = null;
                    return linkType;
                        handle.DangerousRelease();
                    Marshal.FreeHGlobal(outBuffer);
        internal static bool IsHardLink(FileSystemInfo fileInfo)
            return Platform.NonWindowsIsHardLink(fileInfo);
            bool isHardLink = false;
            // only check for hard link if the item is not directory
            if ((fileInfo.Attributes & System.IO.FileAttributes.Directory) != System.IO.FileAttributes.Directory)
                SafeFileHandle handle = Interop.Windows.CreateFileWithSafeFileHandle(fileInfo.FullName, FileAccess.Read, FileShare.Read, FileMode.Open, Interop.Windows.FileAttributes.Normal);
                using (handle)
                    var dangerousHandle = handle.DangerousGetHandle();
                    isHardLink = InternalSymbolicLinkLinkCodeMethods.WinIsHardLink(ref dangerousHandle);
            return isHardLink;
        internal static bool IsReparsePoint(FileSystemInfo fileInfo)
            return fileInfo.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint);
        internal static bool IsReparsePointLikeSymlink(FileSystemInfo fileInfo)
            // Reparse point on Unix is a symlink.
            return IsReparsePoint(fileInfo);
            if (InternalTestHooks.OneDriveTestOn && fileInfo.Name == InternalTestHooks.OneDriveTestSymlinkName)
                return !InternalTestHooks.OneDriveTestRecurseOn;
            Interop.Windows.WIN32_FIND_DATA data = default;
            using (Interop.Windows.SafeFindHandle handle = Interop.Windows.FindFirstFile(fileInfo.FullName, ref data))
                    // Our handle could be invalidated by something else touching the filesystem,
                    // so ensure we deal with that possibility here
                // We already have the file attribute information from our Win32 call,
                // so no need to take the expense of the FileInfo.FileAttributes call
                const int FILE_ATTRIBUTE_REPARSE_POINT = 0x0400;
                if ((data.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) == 0)
                    // Not a reparse point.
                // The name surrogate bit 0x20000000 is defined in https://learn.microsoft.com/windows/win32/fileio/reparse-point-tags
                // Name surrogates (0x20000000) are reparse points that point to other named entities local to the filesystem
                // (like symlinks and mount points).
                // In the case of OneDrive, they are not name surrogates and would be safe to recurse into.
                if ((data.dwReserved0 & 0x20000000) == 0 && (data.dwReserved0 != IO_REPARSE_TAG_APPEXECLINK))
        internal static bool IsSameFileSystemItem(string pathOne, string pathTwo)
            return Platform.NonWindowsIsSameFileSystemItem(pathOne, pathTwo);
            return WinIsSameFileSystemItem(pathOne, pathTwo);
        private static bool WinIsSameFileSystemItem(string pathOne, string pathTwo)
            const Interop.Windows.FileAttributes Attributes = Interop.Windows.FileAttributes.BackupSemantics | Interop.Windows.FileAttributes.PosixSemantics;
            using (var sfOne = Interop.Windows.CreateFileWithSafeFileHandle(pathOne, FileAccess.Read, FileShare.Read, FileMode.Open, Attributes))
            using (var sfTwo = Interop.Windows.CreateFileWithSafeFileHandle(pathTwo, FileAccess.Read, FileShare.Read, FileMode.Open, Attributes))
                if (!sfOne.IsInvalid && !sfTwo.IsInvalid)
                    BY_HANDLE_FILE_INFORMATION infoOne;
                    BY_HANDLE_FILE_INFORMATION infoTwo;
                    if (GetFileInformationByHandle(sfOne.DangerousGetHandle(), out infoOne)
                        && GetFileInformationByHandle(sfTwo.DangerousGetHandle(), out infoTwo))
                        return infoOne.VolumeSerialNumber == infoTwo.VolumeSerialNumber
                               && infoOne.FileIndexHigh == infoTwo.FileIndexHigh
                               && infoOne.FileIndexLow == infoTwo.FileIndexLow;
        internal static bool GetInodeData(string path, out System.ValueTuple<UInt64, UInt64> inodeData)
            bool rv = Platform.NonWindowsGetInodeData(path, out inodeData);
            bool rv = WinGetInodeData(path, out inodeData);
        private static bool WinGetInodeData(string path, out System.ValueTuple<UInt64, UInt64> inodeData)
            using (var sf = Interop.Windows.CreateFileWithSafeFileHandle(path, FileAccess.Read, FileShare.Read, FileMode.Open, Attributes))
                if (!sf.IsInvalid)
                    BY_HANDLE_FILE_INFORMATION info;
                    if (GetFileInformationByHandle(sf.DangerousGetHandle(), out info))
                        UInt64 tmp = info.FileIndexHigh;
                        tmp = (tmp << 32) | info.FileIndexLow;
                        inodeData = (info.VolumeSerialNumber, tmp);
            inodeData = (0, 0);
        internal static bool WinIsHardLink(ref IntPtr handle)
            BY_HANDLE_FILE_INFORMATION handleInfo;
            bool succeeded = InternalSymbolicLinkLinkCodeMethods.GetFileInformationByHandle(handle, out handleInfo);
            return succeeded && (handleInfo.NumberOfLinks > 1);
        internal static bool CreateJunction(string path, string target)
            ArgumentException.ThrowIfNullOrEmpty(target);
            using (SafeHandle handle = WinOpenReparsePoint(path, FileAccess.Write))
                byte[] mountPointBytes = Encoding.Unicode.GetBytes(NonInterpretedPathPrefix + Path.GetFullPath(target));
                var mountPoint = new REPARSE_DATA_BUFFER_MOUNTPOINT();
                mountPoint.ReparseTag = IO_REPARSE_TAG_MOUNT_POINT;
                mountPoint.ReparseDataLength = (ushort)(mountPointBytes.Length + 12); // Added space for the header and null endo
                mountPoint.SubstituteNameOffset = 0;
                mountPoint.SubstituteNameLength = (ushort)mountPointBytes.Length;
                mountPoint.PrintNameOffset = (ushort)(mountPointBytes.Length + 2); // 2 as unicode null take 2 bytes.
                mountPoint.PrintNameLength = 0;
                mountPoint.PathBuffer = new byte[0x3FF0]; // Buffer for max size.
                Array.Copy(mountPointBytes, mountPoint.PathBuffer, mountPointBytes.Length);
                int nativeBufferSize = Marshal.SizeOf(mountPoint);
                IntPtr nativeBuffer = Marshal.AllocHGlobal(nativeBufferSize);
                    Marshal.StructureToPtr(mountPoint, nativeBuffer, false);
                    int bytesReturned = 0;
                    bool result = DeviceIoControl(handle.DangerousGetHandle(), FSCTL_SET_REPARSE_POINT, nativeBuffer, mountPointBytes.Length + 20, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);
                    Marshal.FreeHGlobal(nativeBuffer);
        private static SafeFileHandle WinOpenReparsePoint(string reparsePoint, FileAccess accessMode)
            const Interop.Windows.FileAttributes Attributes = Interop.Windows.FileAttributes.BackupSemantics | Interop.Windows.FileAttributes.OpenReparsePoint;
            SafeFileHandle reparsePointHandle = Interop.Windows.CreateFileWithSafeFileHandle(reparsePoint, accessMode, FileShare.ReadWrite | FileShare.Delete, FileMode.Open, Attributes);
            if (reparsePointHandle.IsInvalid)
                // Save last error since Dispose() will do another pinvoke.
                reparsePointHandle.Dispose();
            return reparsePointHandle;
    #region AlternateDataStreamUtilities
    /// Represents alternate stream data retrieved from a file.
    public class AlternateStreamData
        /// The name of the file that holds this stream.
        public string FileName { get; set; }
        /// The name of this stream.
        /// The length of this stream.
        public long Length { get; set; }
    /// Provides access to alternate data streams on a file.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes",
        Justification = "Needed by both the FileSystem provider and Unblock-File cmdlet.")]
    public static partial class AlternateDataStreamUtilities
        /// List all of the streams on a file.
        /// <param name="path">The fully-qualified path to the file.</param>
        /// <returns>The list of streams (and their size) in the file.</returns>
        internal static List<AlternateStreamData> GetStreams(string path)
            ArgumentNullException.ThrowIfNull(path);
            List<AlternateStreamData> alternateStreams = new List<AlternateStreamData>();
            AlternateStreamNativeData findStreamData = new AlternateStreamNativeData();
            SafeFindHandle handle = NativeMethods.FindFirstStreamW(
                path, NativeMethods.StreamInfoLevels.FindStreamInfoStandard,
                findStreamData, 0);
                // Directories don't normally have alternate streams, so this is not an exceptional state.
                // If a directory has no alternate data streams, FindFirstStreamW returns ERROR_HANDLE_EOF.
                // If the file system (such as FAT32) does not support alternate streams, then
                // ERROR_INVALID_PARAMETER is returned by FindFirstStreamW. See documentation:
                // https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-findfirststreamw
                if (error == NativeMethods.ERROR_HANDLE_EOF || error == NativeMethods.ERROR_INVALID_PARAMETER)
                    return alternateStreams;
                // An unexpected error was returned, that we don't know how to interpret. The most helpful
                // thing we can do at this point is simply throw the raw Win32 exception.
                    // Remove the leading ':'
                    findStreamData.Name = findStreamData.Name.Substring(1);
                    // And trailing :$DATA (as long as it's not the default data stream)
                    const string dataStream = ":$DATA";
                    if (!string.Equals(findStreamData.Name, dataStream, StringComparison.OrdinalIgnoreCase))
                        findStreamData.Name = findStreamData.Name.Replace(dataStream, string.Empty);
                    AlternateStreamData data = new AlternateStreamData();
                    data.Stream = findStreamData.Name;
                    data.Length = findStreamData.Length;
                    data.FileName = path;
                    alternateStreams.Add(data);
                    findStreamData = new AlternateStreamNativeData();
                while (NativeMethods.FindNextStreamW(handle, findStreamData));
                if (lastError != NativeMethods.ERROR_HANDLE_EOF)
            finally { handle.Dispose(); }
        /// Creates a file stream on a file.
        /// <param name="streamName">The name of the alternate data stream to open.</param>
        /// <param name="mode">The FileMode of the file.</param>
        /// <param name="access">The FileAccess of the file.</param>
        /// <param name="share">The FileShare of the file.</param>
        /// <returns>A FileStream that can be used to interact with the file.</returns>
        internal static FileStream CreateFileStream(string path, string streamName, FileMode mode, FileAccess access, FileShare share)
            if (!TryCreateFileStream(path, streamName, mode, access, share, out var stream))
                    FileSystemProviderStrings.AlternateDataStreamNotFound, streamName, path);
                throw new FileNotFoundException(errorMessage, $"{path}:{streamName}");
        /// Tries to create a file stream on a file.
        /// <param name="stream">A FileStream that can be used to interact with the file.</param>
        /// <returns>True if the stream was successfully created, otherwise false.</returns>
        internal static bool TryCreateFileStream(string path, string streamName, FileMode mode, FileAccess access, FileShare share, out FileStream stream)
            ArgumentNullException.ThrowIfNull(streamName);
            if (mode == FileMode.Append)
                mode = FileMode.OpenOrCreate;
            var resultPath = $"{path}:{streamName}";
            SafeFileHandle handle = NativeMethods.CreateFile(resultPath, access, share, IntPtr.Zero, mode, 0, IntPtr.Zero);
                stream = null;
            stream = new FileStream(handle, access);
        /// Removes an alternate data stream.
        /// <param name="path">The path to the file.</param>
        /// <param name="streamName">The name of the alternate data stream to delete.</param>
        internal static void DeleteFileStream(string path, string streamName)
            string adjustedStreamName = streamName.Trim();
            if (!adjustedStreamName.StartsWith(':'))
                adjustedStreamName = ":" + adjustedStreamName;
            string resultPath = path + adjustedStreamName;
            File.Delete(resultPath);
        internal static void SetZoneOfOrigin(string path, SecurityZone securityZone)
            using (FileStream fileStream = CreateFileStream(path, "Zone.Identifier", FileMode.Create, FileAccess.Write, FileShare.None))
            using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.Unicode))
                textWriter.WriteLine("[ZoneTransfer]");
                textWriter.WriteLine("ZoneId={0}", (int)securityZone);
            // an alternative is to use IAttachmentExecute interface as described here:
            // http://joco.name/2010/12/22/windows-antivirus-api-in-net-and-a-com-interop-crash-course/
            // the code above seems cleaner and more robust than the IAttachmentExecute approach
            internal const int ERROR_HANDLE_EOF = 38;
            internal const int ERROR_INVALID_PARAMETER = 87;
            internal enum StreamInfoLevels { FindStreamInfoStandard = 0 }
            [LibraryImport(PinvokeDllNames.CreateFileDllName, EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
            internal static partial SafeFileHandle CreateFile(string lpFileName,
                FileAccess dwDesiredAccess, FileShare dwShareMode,
                IntPtr lpSecurityAttributes, FileMode dwCreationDisposition,
                int dwFlagsAndAttributes, IntPtr hTemplateFile);
            [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "AlternateStreamNativeData.Name")]
            internal static extern SafeFindHandle FindFirstStreamW(
                string lpFileName, StreamInfoLevels InfoLevel,
                [In, Out, MarshalAs(UnmanagedType.LPStruct)]
                AlternateStreamNativeData lpFindStreamData, uint dwFlags);
            internal static extern bool FindNextStreamW(
                SafeFindHandle hndFindFile,
                AlternateStreamNativeData lpFindStreamData);
        internal sealed partial class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
            private SafeFindHandle() : base(true) { }
                return FindClose(this.handle);
            [LibraryImport(PinvokeDllNames.FindCloseDllName)]
            private static partial bool FindClose(IntPtr handle);
        internal class AlternateStreamNativeData
            public long Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)]
    #region CopyFileFromRemoteUtils
    internal static class CopyFileRemoteUtils
        private const string functionToken = "function ";
        private const string nameToken = "Name";
        private const string definitionToken = "Definition";
        #region PSCopyToSessionHelper
        internal const string PSCopyToSessionHelperName = @"PSCopyToSessionHelper";
        private static readonly string s_driveMaxSizeErrorFormatString = FileSystemProviderStrings.DriveMaxSizeError;
        private static readonly string s_PSCopyToSessionHelperDefinition = StringUtil.Format(PSCopyToSessionHelperDefinitionFormat, @"[ValidateNotNullOrEmpty()]", s_driveMaxSizeErrorFormatString);
        private static readonly string s_PSCopyToSessionHelperDefinitionRestricted = StringUtil.Format(PSCopyToSessionHelperDefinitionFormat, @"[ValidateUserDrive()]", s_driveMaxSizeErrorFormatString);
        private const string PSCopyToSessionHelperDefinitionFormat = @"
            [Parameter(ParameterSetName=""PSCopyFileToRemoteSession"")]
            [Parameter(ParameterSetName=""PSCopyAlternateStreamToRemoteSession"")]
            [string] $copyToFilePath,
            [Parameter(ParameterSetName=""PSCopyFileToRemoteSession"", Mandatory=$false)]
            [string] $b64Fragment,
            [switch] $createFile = $false,
            [switch] $emptyFile = $false,
            [Parameter(ParameterSetName=""PSCopyAlternateStreamToRemoteSession"", Mandatory=$true)]
            [string] $streamName,
            [Parameter(ParameterSetName=""PSTargetSupportsAlternateStreams"", Mandatory=$true)]
            [string] $supportAltStreamPath,
            [Parameter(ParameterSetName=""PSSetFileMetadata"", Mandatory=$true)]
            [string] $metaDataFilePath,
            [ValidateNotNull()]
            [hashtable] $metaDataToSet,
            [Parameter(ParameterSetName=""PSRemoteDestinationPathIsFile"", Mandatory=$true)]
            [string] $isFilePath,
            [Parameter(ParameterSetName=""PSGetRemotePathInfo"", Mandatory=$true)]
            [string] $remotePath,
            [Parameter(ParameterSetName=""PSCreateDirectoryOnRemoteSession"", Mandatory=$true)]
            [string] $createDirectoryPath,
            [Parameter(ParameterSetName=""PSCreateDirectoryOnRemoteSession"")]
            [switch] $force
        # Checks if path drive specifies max size and if max size is exceeded
        function CheckPSDriveSize
                [System.Management.Automation.PathInfo] $resolvedPath,
                [int] $fragmentLength
            if (($null -ne $resolvedPath.Drive) -and ($null -ne $resolvedPath.Drive.MaximumSize))
                $maxUserSize = $resolvedPath.Drive.MaximumSize
                $dirSize = 0
                Microsoft.PowerShell.Management\Get-ChildItem -LiteralPath ($resolvedPath.Drive.Name + "":"") -Recurse | ForEach-Object {{
                    Microsoft.PowerShell.Management\Get-Item -LiteralPath $_.FullName -Stream * | ForEach-Object {{ $dirSize += $_.Length }}
                if (($dirSize + $fragmentLength) -gt $maxUserSize)
                    $msg = ""{1}"" -f $maxUserSize
                    throw $msg
        # Return a hashtable with the following members:
        #    BytesWritten - the number of bytes written to a file
        function PSCopyFileToRemoteSession
                [switch] $emptyFile = $false
            $op = @{{
                BytesWritten = $null
            $wstream = $null
                $filePathExists = Microsoft.PowerShell.Management\Test-Path -Path $copyToFilePath
                if ($createFile -or (! $filePathExists))
                    # If the file already exists, try to delete it.
                    if ($filePathExists)
                        Microsoft.PowerShell.Management\Remove-Item -Path $copyToFilePath -Force -ea SilentlyContinue
                    # Create the new file.
                    $fileInfo = Microsoft.PowerShell.Management\New-Item -Path $copyToFilePath -Type File -Force
                    if ($emptyFile)
                        # Handle the empty file scenario.
                        $op['BytesWritten'] = 0
                        return $op
                # Resolve path in case it is a PSDrive
                $resolvedPath = Microsoft.PowerShell.Management\Resolve-Path -literal $copyToFilePath
                # Decode
                $fragment = [System.Convert]::FromBase64String($b64Fragment)
                # Check if drive specifies max size and if max size is exceeded
                CheckPSDriveSize $resolvedPath $fragment.Length
                # Write fragment
                $wstream = Microsoft.PowerShell.Utility\New-Object -TypeName IO.FileStream -ArgumentList ($resolvedPath.ProviderPath), ([System.IO.FileMode]::Append)
                $wstream.Write($fragment, 0, $fragment.Length)
                $op['BytesWritten'] = $fragment.Length
                if ($_.Exception.InnerException)
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception.InnerException
                    Microsoft.PowerShell.Utility\Write-Error -Exception $_.Exception
                if ($null -ne $wstream)
                    $wstream.Dispose()
        # Returns a hashtable with the following members:
        #    BytesWritten - number of bytes written to an alternate file stream
        function PSCopyFileAlternateStreamToRemoteSession
                [string] $streamName
                # Write the stream
                Microsoft.PowerShell.Management\Add-Content -Path ($resolvedPath.ProviderPath) -Value $fragment -Encoding Byte -Stream $streamName -ErrorAction Stop
        # Returns a hashtable with the following member:
        #    TargetSupportsAlternateStreams - boolean to keep track of whether the target supports Alternate data streams.
        function PSTargetSupportsAlternateStreams
                [string] $supportAltStreamPath
            $result = @{{
                TargetSupportsAlternateStreams = $false
            $resolvedPath = Microsoft.PowerShell.Management\Resolve-Path -literal $supportAltStreamPath
            $targetDrive = [IO.Path]::GetPathRoot($resolvedPath.ProviderPath)
            if (-not $targetDrive)
                return $result
            # Check if the target drive is NTFS
            $driveFormat = 'NTFS'
            foreach ($drive in [System.IO.DriveInfo]::GetDrives())
                if (($drive.Name -eq $targetDrive) -and ($drive.DriveFormat -eq $driveFormat))
                    # Now, check if the target supports Add-Command -Stream. This functionality was introduced in version 3.0.
                    $addContentCmdlet = Microsoft.PowerShell.Core\Get-Command Microsoft.PowerShell.Management\Add-Content -ErrorAction SilentlyContinue
                    if ($addContentCmdlet.Parameters.Keys -contains 'Stream')
                        $result['TargetSupportsAlternateStreams'] = $true
        # Sets the metadata for the given file.
        function PSSetFileMetadata
                [hashtable] $metaDataToSet
            $item = Microsoft.PowerShell.Management\get-item $metaDataFilePath -ea SilentlyContinue -Force
            if ($item)
                # LastWriteTime
                if ($metaDataToSet['LastWriteTimeUtc'])
                    $item.LastWriteTimeUtc = $metaDataToSet['LastWriteTimeUtc']
                if ($metaDataToSet['LastWriteTime'])
                    $item.LastWriteTime = $metaDataToSet['LastWriteTime']
                # Attributes
                if ($metaDataToSet['Attributes'])
                    $item.Attributes = $metaDataToSet['Attributes']
        #    IsFileInfo - boolean to keep track of whether the given path is a remote file.
        #    IsDirectoryInfo - boolean to keep track of whether the given path is a remote directory.
        #    ParentIsDirectoryInfo - boolean to keep track of whether the given parent path is a remote directory.
        function PSGetRemotePathInfo
                [string] $remotePath
                    $parentPath = Microsoft.PowerShell.Management\Split-Path $remotePath
                # catch everything and ignore the error.
                catch {{}}
                    IsFileInfo = (Microsoft.PowerShell.Management\Test-Path $remotePath -PathType Leaf)
                    IsDirectoryInfo = (Microsoft.PowerShell.Management\Test-Path $remotePath -PathType Container)
                if ($parentPath)
                    $result['ParentIsDirectoryInfo'] = (Microsoft.PowerShell.Management\Test-Path $parentPath -PathType Container)
        # Returns a hashtable with the following information:
        #  - IsFileInfotrue bool to keep track if the given destination is a FileInfo type.
        function PSRemoteDestinationPathIsFile
                [string] $isFilePath
                IsFileInfo = $null
                $op['IsFileInfo'] = (Microsoft.PowerShell.Management\Test-Path $isFilePath -PathType Leaf)
        # Return a hash table in the following format:
        #      DirectoryPath is the directory to be created.
        #      PathExists is a bool to keep track of whether the directory already exist.
        # 1) If DirectoryPath already exists:
        #     a) If -Force is specified, force create the directory. Set DirectoryPath to the created directory path.
        #     b) If not -Force is specified, then set PathExists to $true.
        # 2) If DirectoryPath does not exist, create it. Set DirectoryPath to the created directory path.
        function PSCreateDirectoryOnRemoteSession
                [switch] $force = $false
                DirectoryPath = $null
                PathExists = $false
                if (Microsoft.PowerShell.Management\Test-Path $createDirectoryPath)
                    # -Force is specified, then force create the directory.
                        Microsoft.PowerShell.Management\New-Item $createDirectoryPath -ItemType Directory -Force | Out-Null
                        $op['DirectoryPath'] = $createDirectoryPath
                        $op['PathExists'] = $true
                    Microsoft.PowerShell.Management\New-Item $createDirectoryPath -ItemType Directory | Out-Null
        # Call helper function based on bound parameter set
        $params = $PSCmdlet.MyInvocation.BoundParameters
        switch ($PSCmdlet.ParameterSetName)
            ""PSCopyFileToRemoteSession""
                return PSCopyFileToRemoteSession @params
            ""PSCopyAlternateStreamToRemoteSession""
                return PSCopyFileAlternateStreamToRemoteSession @params
            ""PSTargetSupportsAlternateStreams""
                return PSTargetSupportsAlternateStreams @params
            ""PSSetFileMetadata""
                return PSSetFileMetadata @params
            ""PSRemoteDestinationPathIsFile""
                return PSRemoteDestinationPathIsFile @params
            ""PSGetRemotePathInfo""
                return PSGetRemotePathInfo @params
            ""PSCreateDirectoryOnRemoteSession""
                return PSCreateDirectoryOnRemoteSession @params
        private static readonly string s_PSCopyToSessionHelper = functionToken + PSCopyToSessionHelperName + @"
        " + s_PSCopyToSessionHelperDefinition + @"
        private static readonly Hashtable s_PSCopyToSessionHelperFunction = new Hashtable() {
            {nameToken, PSCopyToSessionHelperName},
            {definitionToken, s_PSCopyToSessionHelperDefinitionRestricted}
        #region PSCopyFromSessionHelper
        internal const string PSCopyFromSessionHelperName = @"PSCopyFromSessionHelper";
        private static readonly string s_PSCopyFromSessionHelperDefinition = StringUtil.Format(PSCopyFromSessionHelperDefinitionFormat, @"[ValidateNotNullOrEmpty()]");
        private static readonly string s_PSCopyFromSessionHelperDefinitionRestricted = StringUtil.Format(PSCopyFromSessionHelperDefinitionFormat, @"[ValidateUserDrive()]");
        private const string PSCopyFromSessionHelperDefinitionFormat = @"
            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"", Mandatory=$true)]
            [string] $copyFromFilePath,
            [ValidateRange(0, [long]::MaxValue)]
            [long] $copyFromStart,
            [long] $copyFromNumBytes,
            [Parameter(ParameterSetName=""PSCopyFileFromRemoteSession"")]
            [switch] $force,
            [switch] $isAlternateStream,
            [Parameter(ParameterSetName=""PSSourceSupportsAlternateStreams"", Mandatory=$true)]
            [Parameter(ParameterSetName=""PSGetFileMetadata"", Mandatory=$true)]
            [string] $getMetaFilePath,
            [Parameter(ParameterSetName=""PSGetPathItems"", Mandatory=$true)]
            [string] $getPathItems,
            [Parameter(ParameterSetName=""PSGetPathDirAndFiles"", Mandatory=$true)]
            [string] $getPathDir
        # A hash table with the following members is returned:
        #   - moreAvailable bool to keep track of whether there is more data available
        #   - b64Fragment to track of the number of bytes.
        #   - ExceptionThrown bool to keep track if an exception was thrown
        function PSCopyFileFromRemoteSession
                [long] $copyFromNumbytes,
                [switch] $force = $false,
                [switch] $isAlternateStream = $false,
            $finalResult = @{{
                b64Fragment = $null
                moreAvailable = $null
                ExceptionThrown = $false
            function PerformCopyFileFromRemoteSession
                    [string] $filePath,
                    [long] $start,
                    [long] $numBytes,
                    moreAvailable = $false
                # Ensure bytes read is less than Max allowed
                $maxBytes = 10 * 1024 * 1024
                $numBytes = [Math]::Min($numBytes, $maxBytes)
                $rstream = $null
                    if ($isAlternateStream)
                        $content = Microsoft.PowerShell.Management\Get-Content $filePath -stream $streamName -Encoding Byte -Raw
                        $rstream = [System.IO.MemoryStream]::new($content)
                        $rstream = [System.IO.File]::OpenRead($filePath)
                    # Create a new array to hold the file content
                    if ($start -lt $rstream.Length)
                        $o = $rstream.Seek($start, 0)
                        $toRead = [Math]::Min($numBytes, $rstream.Length - $start)
                        $fragment = Microsoft.PowerShell.Utility\New-Object byte[] $toRead
                        $readsoFar = 0
                        while ($readsoFar -lt $toRead)
                            $read = $rstream.Read($fragment, $readSoFar, $toRead - $readsoFar)
                            $readsoFar += $read
                        $op['b64Fragment'] = [System.Convert]::ToBase64String($fragment)
                        if (($start + $readsoFar) -lt $rstream.Length)
                            $op['moreAvailable'] = $true
                    $op
                    if ($null -ne $rstream)
                        $rstream.Dispose()
            function WriteException
                param ($ex)
                if ($ex.Exception.InnerException)
                    Microsoft.PowerShell.Utility\Write-Error -Exception $ex.Exception.InnerException
                    Microsoft.PowerShell.Utility\Write-Error -Exception $ex.Exception
                $finalResult.ExceptionThrown = $true
            $resolvedFilePath = (Microsoft.PowerShell.Management\Resolve-Path -literal $copyFromFilePath).ProviderPath
            $unAuthorizedAccessException = $null
            $result = $null
            $isReadOnly = $false
            $isHidden = $false
                $result = PerformCopyFileFromRemoteSession -filePath $resolvedFilePath -start $copyFromStart -numBytes $copyFromNumBytes -isAlternateStream:$isAlternateStream -streamName $streamName
                $finalResult.b64Fragment =  $result.b64Fragment
                $finalResult.moreAvailable =  $result.moreAvailable
            catch [System.UnauthorizedAccessException]
                $unAuthorizedAccessException = $_
                    $exception = $null
                        # Disable the readonly and hidden attributes and try again
                        $item = Microsoft.PowerShell.Management\Get-Item $resolvedFilePath
                        if ($item.Attributes.HasFlag([System.IO.FileAttributes]::Hidden))
                            $isHidden = $true
                            $item.Attributes = $item.Attributes -band (-bnot ([System.IO.FileAttributes]::Hidden))
                        if ($item.Attributes.HasFlag([System.IO.FileAttributes]::ReadOnly))
                            $isReadOnly = $true
                            $item.Attributes = $item.Attributes -band (-bnot ([System.IO.FileAttributes]::ReadOnly))
                        $result = PerformCopyFileFromRemoteSession -filePath $resolvedFilePath -start $copyFromStart -numBytes $copyFromNumBytes -isAlternateStream:$isAlternateStream
                        $e = $_
                        if (($e.Exception.InnerException -is [System.IO.FileNotFoundException]) -or
                            ($e.Exception.InnerException -is [System.IO.DirectoryNotFoundException]) -or
                            ($e.Exception.InnerException -is [System.Security.SecurityException] ) -or
                            ($e.Exception.InnerException -is [System.ArgumentException]) -or
                            ($e.Exception.InnerException -is [System.IO.IOException]))
                            # Write out the original error since we failed to force the copy
                            WriteException $unAuthorizedAccessException
                            WriteException $e
                WriteException $_
                if ($isReadOnly)
                    $item.Attributes = $item.Attributes -bor [System.IO.FileAttributes]::ReadOnly
                if ($isHidden)
                    $item.Attributes = $item.Attributes -bor [System.IO.FileAttributes]::Hidden
            return $finalResult
        #    SourceSupportsAlternateStreams - boolean to keep track of whether the source supports Alternate data streams.
        #    Streams - the list of alternate streams
        function PSSourceSupportsAlternateStreams
            param ([string]$supportAltStreamPath)
                SourceSupportsAlternateStreams = $false
                Streams = @()
            # Check if the source supports 'Get-Content -Stream'. This functionality was introduced in version 3.0.
            $getContentCmdlet = Microsoft.PowerShell.Core\Get-Command Microsoft.PowerShell.Management\Get-Content -ErrorAction SilentlyContinue
            if ($getContentCmdlet.Parameters.Keys -notcontains 'Stream')
            $result['SourceSupportsAlternateStreams'] = $true
            # Check if the file has any alternate data streams.
            $item = Microsoft.PowerShell.Management\Get-Item -Path $supportAltStreamPath -Stream * -ea SilentlyContinue
            if (-not $item)
            foreach ($streamName in $item.Stream)
                if ($streamName -ne ':$DATA')
                    $result['Streams'] += $streamName
        # Returns a hash table with metadata info about the file for the given path.
        function PSGetFileMetadata
            param ($getMetaFilePath)
            if (-not (Microsoft.PowerShell.Management\Test-Path $getMetaFilePath))
            $item = Microsoft.PowerShell.Management\Get-Item $getMetaFilePath -Force -ea SilentlyContinue
                $metadata = @{{}}
                $attributes = @($item.Attributes.ToString().Split(',').Trim())
                if ($attributes.Count -gt 0)
                    $metadata.Add('Attributes', $attributes)
                $metadata.Add('LastWriteTime', $item.LastWriteTime)
                $metadata.Add('LastWriteTimeUtc', $item.LastWriteTimeUtc)
                return $metadata
        # Converts file system path to PSDrive path
        #    Returns converted path or original path if not conversion is needed.
        function ConvertToPSDrivePath
                [System.Management.Automation.PSDriveInfo] $driveInfo,
                [string] $pathToConvert
            if (!($driveInfo) -or !($driveInfo.Name) -or !($driveInfo.Root))
                return $pathToConvert
            if (! ($driveInfo.Root.StartsWith($driveInfo.Name)))
                return $pathToConvert.ToUpper().Replace($driveInfo.Root.ToUpper(), (($driveInfo.Name) + "":""))
        ## A hashtable is returned in the following format:
        ##  Exists - Boolean to keep track if the given path exists.
        ##  Items  - The items that Get-Item -Path $path resolves to.
        function PSGetPathItems
                [string] $getPathItems
                Exists = $null
                Items = $null
                if (-not (Microsoft.PowerShell.Management\Test-Path $getPathItems))
                    $op['Exists'] = $false
                $items = @(Microsoft.PowerShell.Management\Get-Item -Path $getPathItems | Microsoft.PowerShell.Core\ForEach-Object {{
                        FullName = ConvertToPSDrivePath $_.PSDrive $_.FullName;
                        Name = $_.Name;
                        FileSize = $_.Length; IsDirectory = $_ -is [System.IO.DirectoryInfo]
                }})
                $op['Exists'] = $true
                $op['Items'] = $items
        # Files - Array with file fullnames, and their sizes
        # Directories - Array of child directory fullnames
        function PSGetPathDirAndFiles
            $result = @()
                Files = $null
                Directories = $null
                $item = Microsoft.PowerShell.Management\Get-Item $getPathDir
                if ($item -isnot [System.IO.DirectoryInfo])
                $files = @(Microsoft.PowerShell.Management\Get-ChildItem -Path $getPathDir -File | Microsoft.PowerShell.Core\ForEach-Object {{
                    @{{ FileName = $_.Name;
                        FilePath = (ConvertToPSDrivePath $_.PSDrive $_.FullName);
                        FileSize = $_.Length
                $directories = @(Microsoft.PowerShell.Management\Get-ChildItem -Path $getPathDir -Directory | Microsoft.PowerShell.Core\ForEach-Object {{
                    @{{ Name = $_.Name;
                        FullName = (ConvertToPSDrivePath $_.PSDrive $_.FullName)
                if ($files.count -gt 0)
                    $op['Files'] = $files
                if ($directories.count -gt 0)
                    $op['Directories'] = $directories
            ""PSCopyFileFromRemoteSession""
                return PSCopyFileFromRemoteSession @params
            ""PSSourceSupportsAlternateStreams""
                PSSourceSupportsAlternateStreams @params
            ""PSGetFileMetadata""
                PSGetFileMetadata @params
            ""PSGetPathItems""
                PSGetPathItems @params
            ""PSGetPathDirAndFiles""
                PSGetPathDirAndFiles @params
        internal static readonly string PSCopyFromSessionHelper = functionToken + PSCopyFromSessionHelperName + @"
        " + s_PSCopyFromSessionHelperDefinition + @"
        private static readonly Hashtable s_PSCopyFromSessionHelperFunction = new Hashtable() {
            {nameToken, PSCopyFromSessionHelperName},
            {definitionToken, s_PSCopyFromSessionHelperDefinitionRestricted}
        #region PSCopyRemoteUtils
        internal const string PSCopyRemoteUtilsName = @"PSCopyRemoteUtils";
        internal static readonly string PSCopyRemoteUtilsDefinition = StringUtil.Format(PSCopyRemoteUtilsDefinitionFormat, @"[ValidateNotNullOrEmpty()]", PSValidatePathFunction);
        private static readonly string s_PSCopyRemoteUtilsDefinitionRestricted = StringUtil.Format(PSCopyRemoteUtilsDefinitionFormat, @"[ValidateUserDrive()]", PSValidatePathFunction);
        private const string PSCopyRemoteUtilsDefinitionFormat = @"
            [Parameter(ParameterSetName=""PSRemoteDirectoryExist"", Mandatory=$true)]
            [string] $dirPathExists,
            [Parameter(ParameterSetName=""PSValidatePath"", Mandatory=$true)]
            [string] $pathToValidate,
            [Parameter(ParameterSetName=""PSValidatePath"")]
            [switch] $sourceIsRemote
        #    Exists - boolean to keep track of whether the given path exists for a remote directory.
        function PSRemoteDirectoryExist
                [string] $dirPathExists
            $result = @{{ Exists = (Microsoft.PowerShell.Management\Test-Path $dirPathExists -PathType Container) }}
            ""PSRemoteDirectoryExist""
                return PSRemoteDirectoryExist @params
            ""PSValidatePath""
                return PSValidatePath @params
        private const string PSValidatePathFunction = functionToken + "PSValidatePath" + @"
        " + PSValidatePathDefinition + @"
        internal const string PSValidatePathDefinition = @"
        # Return hashtable in the following format:
        #   Exists - boolean to keep track if the given path exists
        #   Root - the root for the given path. If wildcards are used, it returns the first drive root.
        #   IsAbsolute - boolean to keep track of whether the given path is absolute
        function SafeGetDriveRoot
                [System.Management.Automation.PSDriveInfo] $driveInfo
                return (($driveInfo.Name) + "":"")
                $driveInfo.Root
        $result = @{
            Root = $null
            IsAbsolute = $null
        # Validate if the path is absolute
        $result['IsAbsolute'] = (Microsoft.PowerShell.Management\Split-Path $pathToValidate -IsAbsolute)
        if (-not $result['IsAbsolute'])
        # Check if the given path exists.
        $result['Exists'] = (Microsoft.PowerShell.Management\Test-Path $pathToValidate)
        # If $pathToValidate is a remote source, and it does not exist, return.
        if ($sourceIsRemote -and (-not $result['Exists']))
        # If the path does not exist, check if we can find its root.
        if (-not (Microsoft.PowerShell.Management\Test-Path $pathToValidate))
            $possibleRoot = $null
                $possibleRoot = [System.IO.Path]::GetPathRoot($pathToValidate)
            # Catch everything and ignore the error.
            if (-not $possibleRoot)
            # Now use this path to find its root.
            $pathToValidate = $possibleRoot
        # Get the root path using Get-Item
        $item = Microsoft.PowerShell.Management\Get-Item $pathToValidate -ea SilentlyContinue
        if (($null -ne $item) -and ($item[0].PSProvider.Name -eq 'FileSystem'))
            $result['Root'] = SafeGetDriveRoot $item[0].PSDrive
        # If this fails, try to get them via Get-PSDrive
        $fileSystemDrives = @(Microsoft.PowerShell.Management\Get-PSDrive -PSProvider FileSystem -ea SilentlyContinue)
        # If this fails, try to get them via Get-PSProvider
        if ($fileSystemDrives.Count -eq 0)
            $fileSystemDrives = @((Microsoft.PowerShell.Management\Get-PSProvider -PSProvider FileSystem -ea SilentlyContinue).Drives)
        foreach ($drive in  $fileSystemDrives)
            if ($pathToValidate.StartsWith($drive.Root))
                $result['Root'] = SafeGetDriveRoot $drive
        internal static readonly string PSCopyRemoteUtils = functionToken + PSCopyRemoteUtilsName + @"
        " + PSCopyRemoteUtilsDefinition + @"
        internal static readonly Hashtable PSCopyRemoteUtilsFunction = new Hashtable() {
            {nameToken, PSCopyRemoteUtilsName},
            {definitionToken, s_PSCopyRemoteUtilsDefinitionRestricted}
        internal static readonly string AllCopyToRemoteScripts = s_PSCopyToSessionHelper + PSCopyRemoteUtils;
        internal static IEnumerable<Hashtable> GetAllCopyToRemoteScriptFunctions()
            yield return s_PSCopyToSessionHelperFunction;
            yield return PSCopyRemoteUtilsFunction;
        internal static readonly string AllCopyFromRemoteScripts = PSCopyFromSessionHelper + PSCopyRemoteUtils;
        internal static IEnumerable<Hashtable> GetAllCopyFromRemoteScriptFunctions()
            yield return s_PSCopyFromSessionHelperFunction;

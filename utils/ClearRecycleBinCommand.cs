#if !UNIX
    /// Defines the implementation of the 'Clear-RecycleBin' cmdlet.
    /// This cmdlet clear all files in the RecycleBin for the given DriveLetter.
    /// If not DriveLetter is specified, then the RecycleBin for all drives are cleared.
    [Cmdlet(VerbsCommon.Clear, "RecycleBin", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2109377", ConfirmImpact = ConfirmImpact.High)]
    public class ClearRecycleBinCommand : PSCmdlet
        private string[] _drivesList;
        private DriveInfo[] _availableDrives;
        private bool _force;
        /// Property that sets DriveLetter parameter.
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        public string[] DriveLetter
            get { return _drivesList; }
            set { _drivesList = value; }
        /// Property that sets force parameter. This will allow to clear the recyclebin.
                return _force;
                _force = value;
        /// This method implements the BeginProcessing method for Clear-RecycleBin command.
            _availableDrives = DriveInfo.GetDrives();
        /// This method implements the ProcessRecord method for Clear-RecycleBin command.
            // There are two scenarios:
            // 1) The user provides a list of drives.
            if (_drivesList != null)
                foreach (var drive in _drivesList)
                    if (!IsValidPattern(drive))
                        WriteError(new ErrorRecord(
                            new ArgumentException(
                                string.Format(CultureInfo.InvariantCulture, ClearRecycleBinResources.InvalidDriveNameFormat, "C", "C:", "C:\\")),
                                "InvalidDriveNameFormat",
                                 ErrorCategory.InvalidArgument,
                                 drive));
                    // Get the full path for the drive.
                    string drivePath = GetDrivePath(drive);
                    if (ValidDrivePath(drivePath))
                        EmptyRecycleBin(drivePath);
                // 2) No drivesList is provided by the user.
                EmptyRecycleBin(null);
        /// Returns true if the given drive is 'fixed' and its path exist; otherwise, return false.
        /// <param name="drivePath"></param>
        private bool ValidDrivePath(string drivePath)
            DriveInfo actualDrive = null;
            if (_availableDrives != null)
                foreach (DriveInfo drive in _availableDrives)
                    if (string.Equals(drive.Name, drivePath, StringComparison.OrdinalIgnoreCase))
                        actualDrive = drive;
            // The drive was not found.
            if (actualDrive == null)
                            new System.IO.DriveNotFoundException(
                                string.Format(CultureInfo.InvariantCulture, ClearRecycleBinResources.DriveNotFound, drivePath, "Get-Volume")),
                                "DriveNotFound",
                                drivePath));
                if (actualDrive.DriveType == DriveType.Fixed)
                    // The drive path exists, and the drive is 'fixed'.
                                string.Format(CultureInfo.InvariantCulture, ClearRecycleBinResources.InvalidDriveType, drivePath, "Get-Volume")),
                                "InvalidDriveType",
        /// Returns true if the given input is of the form c, c:, c:\, C, C: or C:\
        /// <param name="input"></param>
        private static bool IsValidPattern(string input)
            return Regex.IsMatch(input, @"^[a-z]{1}$|^[a-z]{1}:$|^[a-z]{1}:\\$", RegexOptions.IgnoreCase);
        /// Returns a drive path of the form C:\ for the given drive driveName.
        /// Supports the following inputs: C, C:, C:\
        /// <param name="driveName"></param>
        private static string GetDrivePath(string driveName)
            string drivePath;
            if (driveName.EndsWith(":\\", StringComparison.OrdinalIgnoreCase))
                drivePath = driveName;
            else if (driveName.EndsWith(':'))
                drivePath = driveName + "\\";
                drivePath = driveName + ":\\";
            return drivePath;
        /// Clear the recyclebin for the given drive name.
        /// If no driveName is provided, it clears the recyclebin for all drives.
        private void EmptyRecycleBin(string drivePath)
            string clearRecycleBinShouldProcessTarget;
            if (drivePath == null)
                clearRecycleBinShouldProcessTarget = string.Format(CultureInfo.InvariantCulture,
                                                                   ClearRecycleBinResources.ClearRecycleBinContent);
                                                                   ClearRecycleBinResources.ClearRecycleBinContentForDrive,
                                                                   drivePath);
            if (_force || (ShouldProcess(clearRecycleBinShouldProcessTarget, "Clear-RecycleBin")))
                // If driveName is null, then clear the recyclebin for all drives; otherwise, just for the specified driveName.
                string activity = string.Format(CultureInfo.InvariantCulture, ClearRecycleBinResources.ClearRecycleBinProgressActivity);
                string statusDescription;
                    statusDescription = string.Format(CultureInfo.InvariantCulture, ClearRecycleBinResources.ClearRecycleBinStatusDescriptionForAllDrives);
                    statusDescription = string.Format(CultureInfo.InvariantCulture, ClearRecycleBinResources.ClearRecycleBinStatusDescriptionByDrive, drivePath);
                ProgressRecord progress = new(0, activity, statusDescription);
                progress.PercentComplete = 30;
                progress.RecordType = ProgressRecordType.Processing;
                WriteProgress(progress);
                // no need to check result as a failure is returned only if recycle bin is already empty
                uint result = NativeMethod.SHEmptyRecycleBin(IntPtr.Zero, drivePath,
                                                            NativeMethod.RecycleFlags.SHERB_NOCONFIRMATION |
                                                            NativeMethod.RecycleFlags.SHERB_NOPROGRESSUI |
                                                            NativeMethod.RecycleFlags.SHERB_NOSOUND);
                progress.PercentComplete = 100;
                progress.RecordType = ProgressRecordType.Completed;
    internal static partial class NativeMethod
        // Internal code to SHEmptyRecycleBin
        internal enum RecycleFlags : uint
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        [LibraryImport("Shell32.dll", StringMarshalling = StringMarshalling.Utf16, EntryPoint = "SHEmptyRecycleBinW")]
        internal static partial uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);

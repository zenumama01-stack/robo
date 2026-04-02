    internal static class PlatformInvokes
        internal sealed class FILETIME
            internal FILETIME()
                dwLowDateTime = 0;
                dwHighDateTime = 0;
            internal FILETIME(long fileTime)
                dwLowDateTime = (uint)fileTime;
                dwHighDateTime = (uint)(fileTime >> 32);
            public long ToTicks()
                return ((long)dwHighDateTime << 32) + dwLowDateTime;
        // dwShareMode of CreateFile
        internal enum FileShareMode : uint
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004,
        // dwCreationDisposition of CreateFile
        internal enum FileCreationDisposition : uint
            TruncateExisting = 5,
            ReadOnly = 0x00000001,
            Directory = 0x00000010,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            SessionAware = 0x00800000
        internal sealed class SecurityAttributes
            internal int nLength;
            internal SafeLocalMemHandle lpSecurityDescriptor;
            internal bool bInheritHandle;
            internal SecurityAttributes()
        /// Creates or opens a file, file stream, directory, physical disk, volume, console buffer,
        /// tape drive, communications resource, mailslot, or named pipe. The function returns a
        /// handle that can be used to access the object.
        /// <param name="lpFileName">
        /// The name of the object to be created or opened.
        /// In the ANSI version of this function, the name is limited to MAX_PATH characters.
        /// To extend this limit to 32,767 wide characters, call the Unicode version of the
        /// function and prepend "\\?\" to the path. For more information, see Naming a File.
        /// For information on special device names, see Defining an MS-DOS Device Name.
        /// To specify a COM port number greater than 9, use the following syntax: "\\.\COM10".
        /// This syntax works for all port numbers and hardware that allows COM port numbers to be specified.
        /// To create a file stream, specify the name of the file, a colon, and then the name of the
        /// stream. For more information, see File Streams.
        /// <param name="dwDesiredAccess">
        /// The access to the object, which can be read, write, or both.
        /// You cannot request an access mode that conflicts with the sharing mode that is
        /// specified in an open request that has an open handle.
        /// If this parameter is zero (0), the application can query file and device attributes
        /// without accessing a device. This is useful for an application to determine the size
        /// of a floppy disk drive and the formats it supports without requiring a floppy in a drive.
        /// It can also be used to test for the existence of a file or directory without opening
        /// them for read or write access.
        /// See the "CreateFile desired access" below.
        /// <param name="dwShareMode">
        /// The sharing mode of an object, which can be read, write, both, or none.
        /// You cannot request a sharing mode that conflicts with the access mode that is specified
        /// in an open request that has an open handle, because that would result in the following
        /// sharing violation: ERROR_SHARING_VIOLATION.
        /// If this parameter is zero (0) and CreateFile succeeds, the object cannot be shared
        /// and cannot be opened again until the handle is closed. For more information, see the
        /// Remarks section of this topic.
        /// The sharing options remain in effect until you close the handle to an object.
        /// To enable a process to share an object while another process has the object open,
        /// use a combination of one or more of the following values to specify the access mode
        /// they can request to open the object.
        /// <param name="lpSecurityAttributes">
        /// A pointer to a SECURITY_ATTRIBUTES structure that determines whether or not the returned
        /// handle can be inherited by child processes.
        /// If lpSecurityAttributes is NULL, the handle cannot be inherited.
        /// The lpSecurityDescriptor member of the structure specifies a security descriptor
        /// for an object. If lpSecurityAttributes is NULL, the object gets a default security descriptor.
        /// The access control lists (ACL) in the default security descriptor for a file or directory
        /// are inherited from its parent directory.
        /// The target file system must support security on files and directories for this parameter to
        /// have an effect on them, which is indicated when GetVolumeInformation returns FS_PERSISTENT_ACLS.
        /// CreateFile ignores lpSecurityDescriptor when opening an existing file, but continues to
        /// use the other structure members.
        /// <param name="dwCreationDisposition">
        /// An action to take on files that exist and do not exist.
        /// See "CreateFile creation disposition" below
        /// <param name="dwFlagsAndAttributes">
        /// The file attributes and flags.
        /// This parameter can include any combination of the file attributes.
        /// All other file attributes override FILE_ATTRIBUTE_NORMAL.
        /// When CreateFile opens a file, it combines the file flags with existing
        /// file attributes, and ignores any supplied file attributes.
        /// <param name="hTemplateFile">
        /// A handle to a template file with the GENERIC_READ access right.
        /// The template file supplies file attributes and extended attributes for the file that is
        /// being created. This parameter can be NULL.
        /// When opening an existing file, CreateFile ignores the template file.
        /// When opening a new EFS-encrypted file, the file inherits the DACL from its parent directory.
        /// If the function succeeds, the return value is an open handle to a specified file.
        /// If a specified file exists before the function call and dwCreationDisposition is CREATE_ALWAYS
        /// or OPEN_ALWAYS, a call to GetLastError returns ERROR_ALREADY_EXISTS, even when the
        /// function succeeds. If a file does not exist before the call, GetLastError returns zero (0).
        /// If the function fails, the return value is INVALID_HANDLE_VALUE.
        /// To get extended error information, call GetLastError.
        internal static extern IntPtr CreateFile(
            FileDesiredAccess dwDesiredAccess,
            FileShareMode dwShareMode,
            IntPtr lpSecurityAttributes,
            FileCreationDisposition dwCreationDisposition,
        /// Closes an open object handle.
        /// <param name="handle">
        /// A valid handle to an open object.
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information,
        /// call GetLastError.
        /// If the application is running under a debugger, the function will throw an exception
        /// if it receives either a handle value that is not valid or a pseudo-handle value.
        /// This can happen if you close a handle twice, or if you call CloseHandle on a handle
        /// returned by the FindFirstFile function.
        [DllImport(PinvokeDllNames.DosDateTimeToFileTimeDllName, SetLastError = false)]
        internal static extern bool DosDateTimeToFileTime(
            short wFatDate, // _In_   WORD
            short wFatTime, // _In_   WORD
            FILETIME lpFileTime); // _Out_ LPFILETIME
        [DllImport(PinvokeDllNames.LocalFileTimeToFileTimeDllName, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern bool LocalFileTimeToFileTime(
            FILETIME lpLocalFileTime, // _In_   const FILETIME *
        [DllImport(PinvokeDllNames.SetFileTimeDllName, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern bool SetFileTime(
            IntPtr hFile, // _In_      HANDLE
            FILETIME lpCreationTime, // _In_opt_ const FILETIME *
            FILETIME lpLastAccessTime, // _In_opt_ const FILETIME *
            FILETIME lpLastWriteTime); // _In_opt_ const FILETIME *
        [DllImport(PinvokeDllNames.SetFileAttributesWDllName, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern bool SetFileAttributesW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName, // _In_ LPCTSTR
            FileAttributes dwFileAttributes); // _In_ DWORD
        /// Enable the privilege specified by the privilegeName. If the specified privilege is already enabled, return true
        /// with the oldPrivilegeState.PrivilegeCount set to 0. Otherwise, enable the specified privilege, and the old privilege
        /// state will be saved in oldPrivilegeState.
        /// <param name="privilegeName"></param>
        /// <param name="oldPrivilegeState"></param>
        internal static bool EnableTokenPrivilege(string privilegeName, ref TOKEN_PRIVILEGE oldPrivilegeState)
            TOKEN_PRIVILEGE newPrivilegeState = new TOKEN_PRIVILEGE();
            // Check if the caller has the specified privilege or not
            if (LookupPrivilegeValue(null, privilegeName, ref newPrivilegeState.Privilege.Luid))
                // Get the pseudo handler of the current process
                IntPtr processHandler = GetCurrentProcess();
                if (processHandler != IntPtr.Zero)
                    // Get the handler of the current process's access token
                    IntPtr tokenHandler = IntPtr.Zero;
                    if (OpenProcessToken(processHandler, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out tokenHandler))
                        // Check if the specified privilege is already enabled
                        PRIVILEGE_SET requiredPrivilege = new PRIVILEGE_SET();
                        requiredPrivilege.Privilege.Luid = newPrivilegeState.Privilege.Luid;
                        requiredPrivilege.PrivilegeCount = 1;
                        // PRIVILEGE_SET_ALL_NECESSARY is defined as 1
                        requiredPrivilege.Control = 1;
                        bool privilegeEnabled = false;
                        if (PrivilegeCheck(tokenHandler, ref requiredPrivilege, out privilegeEnabled) && privilegeEnabled)
                            // The specified privilege is already enabled
                            oldPrivilegeState.PrivilegeCount = 0;
                            // The specified privilege is not enabled yet. Enable it.
                            newPrivilegeState.PrivilegeCount = 1;
                            newPrivilegeState.Privilege.Attributes = SE_PRIVILEGE_ENABLED;
                            int bufferSize = Marshal.SizeOf<TOKEN_PRIVILEGE>();
                            int returnSize = 0;
                            // enable the specified privilege
                            if (AdjustTokenPrivileges(tokenHandler, false, ref newPrivilegeState, bufferSize, out oldPrivilegeState, ref returnSize))
                                // AdjustTokenPrivileges returns true does not mean all specified privileges have been successfully enabled
                                int retCode = Marshal.GetLastWin32Error();
                                if (retCode == ERROR_SUCCESS)
                                else if (retCode == 1300)
                                    // 1300 - Not all privileges referenced are assigned to the caller. This means the specified privilege is not
                                    // assigned to the current user. For example, suppose the role of current caller is "User", then privilege "SeRemoteShutdownPrivilege"
                                    // is not assigned to the role. In this case, we just return true and leave the call to "Win32Shutdown" to decide
                                    // whether the permission is granted or not.
                                    // Set oldPrivilegeState.PrivilegeCount to 0 to avoid the privilege restore later (PrivilegeCount - how many privileges are modified)
                    // Close the token handler and the process handler
                    if (tokenHandler != IntPtr.Zero)
                        CloseHandle(tokenHandler);
                    CloseHandle(processHandler);
        /// Restore the previous privilege state.
        /// <param name="previousPrivilegeState"></param>
        internal static bool RestoreTokenPrivilege(string privilegeName, ref TOKEN_PRIVILEGE previousPrivilegeState)
            // The privilege was not changed, do not need to restore it.
            if (previousPrivilegeState.PrivilegeCount == 0)
            TOKEN_PRIVILEGE newState = new TOKEN_PRIVILEGE();
            // Check if the caller has the specified privilege or not. If the caller has it, check the LUID specified in previousPrivilegeState
            // to see if the previousPrivilegeState is defined for the same privilege
            if (LookupPrivilegeValue(null, privilegeName, ref newState.Privilege.Luid) &&
                newState.Privilege.Luid.HighPart == previousPrivilegeState.Privilege.Luid.HighPart &&
                newState.Privilege.Luid.LowPart == previousPrivilegeState.Privilege.Luid.LowPart)
                        // restore the privilege state back to the previous privilege state
                        if (AdjustTokenPrivileges(tokenHandler, false, ref previousPrivilegeState, bufferSize, out newState, ref returnSize))
                            if (Marshal.GetLastWin32Error() == ERROR_SUCCESS)
        /// The LookupPrivilegeValue function retrieves the locally unique identifier (LUID) used on a specified system to locally represent
        /// the specified privilege name.
        [DllImport(PinvokeDllNames.LookupPrivilegeValueDllName, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);
        /// The PrivilegeCheck function determines whether a specified privilege is enabled in an access token.
        /// <param name="tokenHandler"></param>
        /// <param name="requiredPrivileges"></param>
        [DllImport(PinvokeDllNames.PrivilegeCheckDllName, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        internal static extern bool PrivilegeCheck(IntPtr tokenHandler, ref PRIVILEGE_SET requiredPrivileges, out bool pfResult);
        /// The AdjustTokenPrivileges function enables or disables privileges in the specified access token. Enabling or disabling privileges in
        /// an access token requires TOKEN_ADJUST_PRIVILEGES access. The TOKEN_ADJUST_PRIVILEGES and TOKEN_QUERY accesses are gained when calling
        /// the OpenProcessToken function.
        /// <param name="disableAllPrivilege"></param>
        /// <param name="newPrivilegeState"></param>
        /// <param name="bufferLength"></param>
        [DllImport(PinvokeDllNames.AdjustTokenPrivilegesDllName, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        internal static extern bool AdjustTokenPrivileges(IntPtr tokenHandler, bool disableAllPrivilege,
                                                          ref TOKEN_PRIVILEGE newPrivilegeState, int bufferLength,
                                                          out TOKEN_PRIVILEGE previousPrivilegeState,
                                                          ref int returnLength);
        internal struct PRIVILEGE_SET
            internal uint Control;
        /// Get the pseudo handler of the current process.
        [DllImport(PinvokeDllNames.GetCurrentProcessDllName)]
        /// This function exists just for backward compatibility. It is preferred to use the other override that takes 'SafeHandle' as parameter.
        /// Required to enable or disable the privileges in an access token.
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        /// Required to query an access token.
        internal const int TOKEN_QUERY = 0x00000008;
        /// Combines all possible access rights for a token.
        internal const int TOKEN_ALL_ACCESS = 0x001f01ff;
        internal const uint SE_PRIVILEGE_DISABLED = 0x00000000;
        #region CreateProcess for SSH Remoting
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        internal static readonly UInt32 GENERIC_READ = 0x80000000;
        internal static readonly UInt32 GENERIC_WRITE = 0x40000000;
        internal static readonly UInt32 FILE_ATTRIBUTE_NORMAL = 0x80000000;
        internal static readonly UInt32 CREATE_ALWAYS = 2;
        internal static readonly UInt32 FILE_SHARE_WRITE = 0x00000002;
        internal static readonly UInt32 FILE_SHARE_READ = 0x00000001;
        internal static readonly UInt32 OF_READWRITE = 0x00000002;
        internal static readonly UInt32 OPEN_EXISTING = 3;
        internal sealed class PROCESS_INFORMATION
            public PROCESS_INFORMATION()
                this.hProcess = IntPtr.Zero;
                this.hThread = IntPtr.Zero;
                    if (this.hProcess != IntPtr.Zero)
                        CloseHandle(this.hProcess);
                    if (this.hThread != IntPtr.Zero)
                        CloseHandle(this.hThread);
        internal static extern bool CreateProcess(
            [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
            PROCESS_INFORMATION lpProcessInformation);
        internal static readonly uint RESUME_THREAD_FAILED = System.UInt32.MaxValue; // (DWORD)-1

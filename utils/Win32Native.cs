 * Notes to PInvoke users:  Getting the syntax exactly correct is crucial, and
 * more than a little confusing.  Here's some guidelines.
 * For handles, you should use a SafeHandle subclass specific to your handle
 * type.
     * Win32 encapsulation for MSCORLIB.
    // Remove the default demands for all N/Direct methods with this
    // global declaration on the class.
    [SuppressUnmanagedCodeSecurity]
    internal static class Win32Native
        #region Integer Const
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7A;
        #endregion Integer Const
        #region Enum
        internal enum TOKEN_INFORMATION_CLASS
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin
            SidTypeUser = 1,
            SidTypeComputer
        #endregion Enum
        #region Struct
        internal struct SID_AND_ATTRIBUTES
            internal IntPtr Sid;
            internal uint Attributes;
        internal struct TOKEN_USER
            internal SID_AND_ATTRIBUTES User;
        #endregion Struct
        #region PInvoke methods
        /// The LookupAccountSid function accepts a security identifier (SID) as input. It retrieves the name
        /// of the account for this SID and the name of the first domain on which this SID is found.
        /// <param name="sid"></param>
        /// <param name="cchName"></param>
        /// <param name="referencedDomainName"></param>
        /// <param name="cchReferencedDomainName"></param>
        /// <param name="peUse"></param>
        [DllImport(PinvokeDllNames.LookupAccountSidDllName, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        private static extern unsafe bool LookupAccountSid(string lpSystemName,
                                                     IntPtr sid,
                                                     char* lpName,
                                                     ref int cchName,
                                                     char* referencedDomainName,
                                                     ref int cchReferencedDomainName,
        internal static unsafe bool LookupAccountSid(string lpSystemName,
                                                     Span<char> userName,
                                                     Span<char> domainName,
                                                     ref int cchDomainName,
                                                     out SID_NAME_USE peUse)
            fixed (char* userNamePtr = &MemoryMarshal.GetReference(userName))
            fixed (char* domainNamePtr = &MemoryMarshal.GetReference(domainName))
                return LookupAccountSid(lpSystemName,
                                        userNamePtr,
                                        ref cchName,
                                        domainNamePtr,
                                        ref cchDomainName,
                                        out peUse);
        [DllImport(PinvokeDllNames.CloseHandleDllName, SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);
        /// Retrieves the current process token.
        /// <param name="processHandle">Process handle.</param>
        /// <param name="desiredAccess">Token access.</param>
        /// <param name="tokenHandle">Process token.</param>
        /// <returns>The current process token.</returns>
        [DllImport(PinvokeDllNames.OpenProcessTokenDllName, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        internal static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        /// The GetTokenInformation function retrieves a specified type of information about an access token.
        /// The calling process must have appropriate access rights to obtain the information.
        /// <param name="tokenHandle"></param>
        /// <param name="tokenInformationClass"></param>
        /// <param name="tokenInformation"></param>
        /// <param name="tokenInformationLength"></param>
        /// <param name="returnLength"></param>
        [DllImport(PinvokeDllNames.GetTokenInformationDllName, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        internal static extern bool GetTokenInformation(IntPtr tokenHandle,
                                                        TOKEN_INFORMATION_CLASS tokenInformationClass,
                                                        IntPtr tokenInformation,
                                                        int tokenInformationLength,
        #endregion PInvoke Methods

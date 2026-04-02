//  Application white listing policies such as AppLocker and DeviceGuard UMCI are only implemented on Windows OSs
    // Internal Note: Current code that consumes this enum assumes that anything but 'Enforce' means
    // that the script is allowed, and that a system lockdown policy that is anything but 'None' means
    // that the API should be called again for individual files. If any elements are added to this enum,
    // callers of the GetLockdownPolicy() should be reviewed.
    /// Support class for dealing with the Windows Lockdown Policy,
    /// Device Guard, and Constrained PowerShell.
        private SystemPolicy()
            string messageToWrite = message;
            // Augment the log message with current script information from the script debugger, if available.
            bool debuggerAvailable = context is not null &&
                                     context._debugger is ScriptDebugger;
            if (debuggerAvailable)
                var scriptPosMessage = context._debugger.GetCurrentScriptPosition();
                if (!string.IsNullOrEmpty(scriptPosMessage))
                    messageToWrite = message + scriptPosMessage;
            PSEtwLog.LogWDACAuditEvent(title, messageToWrite, fqid);
            // We drop into the debugger only if requested and we are running in the interactive host session runspace (Id == 1).
            if (debuggerAvailable && dropIntoDebugger &&
                context._debugger.DebugMode.HasFlag(DebugModes.LocalScript) &&
                Runspace.DefaultRunspace?.Id == 1 &&
                context.DebugPreferenceVariable.HasFlag(ActionPreference.Break) &&
                context.InternalHost?.UI is not null)
                    context.InternalHost.UI.WriteLine();
                    context.InternalHost.UI.WriteLine("WDAC Audit Log:");
                    context.InternalHost.UI.WriteLine($"Title: {title}");
                    context.InternalHost.UI.WriteLine($"Message: {message}");
                    context.InternalHost.UI.WriteLine($"FullyQualifedId: {fqid}");
                    context.InternalHost.UI.WriteLine("Stopping script execution in debugger...");
                    context._debugger.Break();
        /// <returns>An EnforcementMode that describes the system policy.</returns>
            if (s_systemLockdownPolicy == null)
                lock (s_systemLockdownPolicyLock)
                    s_systemLockdownPolicy ??= GetLockdownPolicy(path: null, handle: null);
            else if (s_allowDebugOverridePolicy)
                    s_systemLockdownPolicy = GetDebugLockdownPolicy(path: null, out _);
            return s_systemLockdownPolicy.Value;
        private static readonly object s_systemLockdownPolicyLock = new object();
        private static SystemEnforcementMode? s_systemLockdownPolicy = null;
        private static bool s_allowDebugOverridePolicy = false;
        private static bool s_wldpCanExecuteAvailable = true;
            SafeHandle fileHandle = fileStream.SafeFileHandle;
            SystemEnforcementMode systemLockdownPolicy = GetSystemLockdownPolicy();
            // First check latest WDAC APIs if available.
            if (systemLockdownPolicy is SystemEnforcementMode.Enforce
                && s_wldpCanExecuteAvailable
                && TryGetWldpCanExecuteFileResult(filePath, fileHandle, out SystemScriptFileEnforcement wldpFilePolicy))
                return GetLockdownPolicy(filePath, fileHandle, wldpFilePolicy);
            // Failed to invoke WldpCanExecuteFile, revert to legacy APIs.
            if (systemLockdownPolicy is SystemEnforcementMode.None)
            // WldpCanExecuteFile was invoked successfully so we can skip running
            // legacy WDAC APIs. AppLocker must still be checked in case it is more
            // strict than the current WDAC policy.
            return GetLockdownPolicy(filePath, fileHandle, canExecuteResult: null);
        private static SystemScriptFileEnforcement ConvertToModernFileEnforcement(SystemEnforcementMode legacyMode)
            return legacyMode switch
                SystemEnforcementMode.None => SystemScriptFileEnforcement.Allow,
                SystemEnforcementMode.Audit => SystemScriptFileEnforcement.AllowConstrainedAudit,
                SystemEnforcementMode.Enforce => SystemScriptFileEnforcement.AllowConstrained,
                _ => SystemScriptFileEnforcement.Block,
        private static bool TryGetWldpCanExecuteFileResult(string filePath, SafeHandle fileHandle, out SystemScriptFileEnforcement result)
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string auditMsg = $"PowerShell ExternalScriptInfo reading file: {fileName}";
                int hr = WldpNativeMethods.WldpCanExecuteFile(
                    host: PowerShellHost,
                    options: WLDP_EXECUTION_EVALUATION_OPTIONS.WLDP_EXECUTION_EVALUATION_OPTION_NONE,
                    fileHandle: fileHandle.DangerousGetHandle(),
                    auditInfo: auditMsg,
                    result: out WLDP_EXECUTION_POLICY canExecuteResult);
                PSEtwLog.LogWDACQueryEvent("WldpCanExecuteFile", filePath, hr, (int)canExecuteResult);
                if (hr >= 0)
                    switch (canExecuteResult)
                        case WLDP_EXECUTION_POLICY.WLDP_CAN_EXECUTE_ALLOWED:
                            result = SystemScriptFileEnforcement.Allow;
                        case WLDP_EXECUTION_POLICY.WLDP_CAN_EXECUTE_BLOCKED:
                            result = SystemScriptFileEnforcement.Block;
                        case WLDP_EXECUTION_POLICY.WLDP_CAN_EXECUTE_REQUIRE_SANDBOX:
                            result = SystemScriptFileEnforcement.AllowConstrained;
                            // Fall through to legacy system policy checks.
                            Debug.Assert(false, $"Unknown policy result returned from WldCanExecute: {canExecuteResult}");
                // If HResult is unsuccessful (such as E_NOTIMPL (0x80004001)), fall through to legacy system checks.
            catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
                // Fall back to legacy system policy checks.
                s_wldpCanExecuteAvailable = false;
                PSEtwLog.LogWDACQueryEvent("WldpCanExecuteFile_Failed", filePath, ex.HResult, 0);
            result = default;
        /// <returns>An EnforcementMode that describes policy.</returns>
        public static SystemEnforcementMode GetLockdownPolicy(string path, SafeHandle handle)
            SystemScriptFileEnforcement modernMode = GetLockdownPolicy(path, handle, canExecuteResult: null);
                modernMode is not SystemScriptFileEnforcement.Block,
                "Block should never be converted to legacy file enforcement.");
            return modernMode switch
                SystemScriptFileEnforcement.Block => SystemEnforcementMode.Enforce,
                SystemScriptFileEnforcement.AllowConstrained => SystemEnforcementMode.Enforce,
                SystemScriptFileEnforcement.AllowConstrainedAudit => SystemEnforcementMode.Audit,
                SystemScriptFileEnforcement.Allow => SystemEnforcementMode.None,
                SystemScriptFileEnforcement.None => SystemEnforcementMode.None,
                _ => throw new ArgumentOutOfRangeException(nameof(modernMode)),
        private static SystemScriptFileEnforcement GetLockdownPolicy(
            SafeHandle handle,
            SystemScriptFileEnforcement? canExecuteResult)
            SystemScriptFileEnforcement wldpFilePolicy = canExecuteResult
                ?? ConvertToModernFileEnforcement(GetWldpPolicy(path, handle));
            // Check the WLDP File policy via API
            if (wldpFilePolicy is SystemScriptFileEnforcement.Block or SystemScriptFileEnforcement.AllowConstrained)
                return wldpFilePolicy;
            // Check the AppLocker File policy via API
            // This needs to be checked before WLDP audit policy
            // So, that we don't end up in Audit mode,
            // when we should be enforce mode.
            var appLockerFilePolicy = GetAppLockerPolicy(path, handle);
            if (appLockerFilePolicy == SystemEnforcementMode.Enforce)
                return ConvertToModernFileEnforcement(appLockerFilePolicy);
            // At this point, LockdownPolicy = Audit or Allowed.
            // If there was a WLDP policy, but WLDP didn't block it,
            // then it was explicitly allowed. Therefore, return the result for the file.
            if (s_cachedWldpSystemPolicy is SystemEnforcementMode.Audit or SystemEnforcementMode.Enforce
                || wldpFilePolicy is SystemScriptFileEnforcement.AllowConstrainedAudit)
            // If there was a system-wide AppLocker policy, but AppLocker didn't block it,
            // then return AppLocker's status.
            if (s_cachedSaferSystemPolicy is SaferPolicy.Disallowed)
            // If it's not set to 'Enforce' by the platform, allow debug overrides
            GetDebugLockdownPolicy(path, out SystemScriptFileEnforcement debugPolicy);
            return debugPolicy;
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods",
            MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
        private static SystemEnforcementMode GetWldpPolicy(string path, SafeHandle handle)
            // If the WLDP assembly is missing (such as windows 7 or down OS), return default/None to skip WLDP validation
            if (s_hadMissingWldpAssembly)
                return s_cachedWldpSystemPolicy.GetValueOrDefault(SystemEnforcementMode.None);
            // If path is NULL, see if we have the cached system-wide lockdown policy.
                if ((s_cachedWldpSystemPolicy != null) && (!InternalTestHooks.BypassAppLockerPolicyCaching))
                    return s_cachedWldpSystemPolicy.Value;
                WLDP_HOST_INFORMATION hostInformation = new WLDP_HOST_INFORMATION();
                hostInformation.dwRevision = WldpNativeConstants.WLDP_HOST_INFORMATION_REVISION;
                hostInformation.dwHostId = WLDP_HOST_ID.WLDP_HOST_ID_POWERSHELL;
                    hostInformation.szSource = path;
                        IntPtr fileHandle = IntPtr.Zero;
                        fileHandle = handle.DangerousGetHandle();
                        hostInformation.hSource = fileHandle;
                uint pdwLockdownState = 0;
                int result = WldpNativeMethods.WldpGetLockdownPolicy(ref hostInformation, ref pdwLockdownState, 0);
                PSEtwLog.LogWDACQueryEvent("WldpGetLockdownPolicy", path, result, (int)pdwLockdownState);
                    SystemEnforcementMode resultingLockdownPolicy = GetLockdownPolicyForResult(pdwLockdownState);
                    // If this is a query for the system-wide lockdown policy, cache it.
                        s_cachedWldpSystemPolicy = resultingLockdownPolicy;
                    return resultingLockdownPolicy;
                    // API failure?
                    return SystemEnforcementMode.Enforce;
                s_hadMissingWldpAssembly = true;
                PSEtwLog.LogWDACQueryEvent("WldpGetLockdownPolicy_Failed", path, ex.HResult, 0);
        private static SystemEnforcementMode? s_cachedWldpSystemPolicy = null;
        private const string AppLockerTestFileName = "__PSScriptPolicyTest_";
        private const string AppLockerTestFileContents = "# PowerShell test file to determine AppLocker lockdown mode ";
        private static SystemEnforcementMode GetAppLockerPolicy(string path, SafeHandle handle)
            SaferPolicy result = SaferPolicy.Disallowed;
            // If path is NULL, we're looking for the system-wide lockdown policy.
            // Since there is no way to get that from AppLocker, we will test the policy
            // against a random non-existent script and module. If that is allowed, then there is
            // no AppLocker script policy.
                if ((s_cachedSaferSystemPolicy != null) && (!InternalTestHooks.BypassAppLockerPolicyCaching))
                    result = s_cachedSaferSystemPolicy.Value;
                    // Temp path can sometimes be deleted. While many places in PowerShell depend on its existence,
                    // this one can crash PowerShell.
                    // A less sensitive implementation will be possible once AppLocker allows validation of files that
                    // don't exist.
                    string testPathScript = null;
                    string testPathModule = null;
                        // Start with the current profile temp path.
                        string tempPath = IO.Path.GetTempPath();
                        while (iteration++ < 2)
                                if (!IO.Directory.Exists(tempPath))
                                    IO.Directory.CreateDirectory(tempPath);
                                testPathScript = IO.Path.Combine(tempPath, AppLockerTestFileName + IO.Path.GetRandomFileName() + ".ps1");
                                testPathModule = IO.Path.Combine(tempPath, AppLockerTestFileName + IO.Path.GetRandomFileName() + ".psm1");
                                // AppLocker fails when you try to check a policy on a file
                                // with no content. So create a scratch file and test on that.
                                string dtAppLockerTestFileContents = AppLockerTestFileContents + Environment.TickCount64;
                                IO.File.WriteAllText(testPathScript, dtAppLockerTestFileContents);
                                IO.File.WriteAllText(testPathModule, dtAppLockerTestFileContents);
                            catch (IO.IOException)
                                if (iteration == 2)
                            if (!error)
                            // Try again with the AppData\LocalLow\Temp path using known folder id:
                            // https://msdn.microsoft.com/library/dd378457.aspx
                            Guid AppDatalocalLowFolderId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");
                            tempPath = GetKnownFolderPath(AppDatalocalLowFolderId) + @"\Temp";
                        // Test policy.
                        result = TestSaferPolicy(testPathScript, testPathModule);
                        // If we fail to test the policy, assume the default.
                        result = SaferPolicy.Disallowed;
                        // This can happen during thread impersonation if the profile temp paths are not accessible.
                        // Allow policy if impersonated, otherwise disallow.
                            (System.Security.Principal.WindowsIdentity.GetCurrent().ImpersonationLevel == System.Security.Principal.TokenImpersonationLevel.Impersonation) ?
                            SaferPolicy.Allowed : SaferPolicy.Disallowed;
                        // This is for IO.Path.GetTempPath() call when temp paths are not accessible.
                        // Ok to leave the test scripts in the temp folder if they happen to be in use
                        // so that PowerShell will still startup.
                        PathUtils.TryDeleteFile(testPathScript);
                        PathUtils.TryDeleteFile(testPathModule);
                    s_cachedSaferSystemPolicy = result;
                if (result == SaferPolicy.Disallowed)
                // We got a path. Return the result for that path.
                result = SecuritySupport.GetSaferPolicy(path, handle);
        private static SaferPolicy? s_cachedSaferSystemPolicy = null;
        private static string GetKnownFolderPath(Guid knownFolderId)
            IntPtr pszPath = IntPtr.Zero;
                int hr = WldpNativeMethods.SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                    return Marshal.PtrToStringAuto(pszPath);
                throw new System.IO.IOException();
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
        private static SaferPolicy TestSaferPolicy(string testPathScript, string testPathModule)
            SaferPolicy result = SecuritySupport.GetSaferPolicy(testPathScript, null);
                result = SecuritySupport.GetSaferPolicy(testPathModule, null);
        private static SystemEnforcementMode GetDebugLockdownPolicy(string path, out SystemScriptFileEnforcement modernEnforcement)
            s_allowDebugOverridePolicy = true;
            // Support fall-back debug hook for path exclusions on non-WOA platforms
                // Assume everything under SYSTEM32 is trusted, with a purposefully sloppy
                // check so that we can actually put it in the filename during testing.
                if (path.Contains("System32", StringComparison.OrdinalIgnoreCase))
                    modernEnforcement = SystemScriptFileEnforcement.Allow;
                // No explicit debug allowance for the file, so return the system policy if there is one.
                modernEnforcement = s_systemLockdownPolicy switch
                    SystemEnforcementMode.None => SystemScriptFileEnforcement.None,
                    _ => SystemScriptFileEnforcement.None,
                return s_systemLockdownPolicy.GetValueOrDefault(SystemEnforcementMode.None);
            // Support fall-back debug hook for system-wide policy on non-WOA platforms
            object result = Environment.GetEnvironmentVariable("__PSLockdownPolicy", EnvironmentVariableTarget.Machine);
                pdwLockdownState = LanguagePrimitives.ConvertTo<uint>(result);
                SystemEnforcementMode policy = GetLockdownPolicyForResult(pdwLockdownState);
                modernEnforcement = ConvertToModernFileEnforcement(policy);
            // If the system-wide debug policy had no preference, then there is no enforcement.
            modernEnforcement = SystemScriptFileEnforcement.None;
        private static bool s_hadMissingWldpAssembly = false;
        /// Gets lockdown policy as applied to a COM object.
        /// <returns>True if the COM object is allowed, False otherwise.</returns>
            // This method is called only if there is an AppLocker and/or WLDP system wide lock down enforcement policy.
            if (s_cachedWldpSystemPolicy.GetValueOrDefault(SystemEnforcementMode.None) != SystemEnforcementMode.Enforce)
                // No WLDP policy implies only AppLocker policy enforcement. Disallow all COM object instantiation.
            // WLDP policy must be in system wide enforcement, look up COM Id in WLDP approval list.
                int pIsApproved = 0;
                int result = WldpNativeMethods.WldpIsClassInApprovedList(ref clsid, ref hostInformation, ref pIsApproved, 0);
                    if (pIsApproved == 1)
                        // Hook for testability. If we've got an environmental override, say that ADODB.Parameter
                        // is not allowed.
                        // 0000050b-0000-0010-8000-00aa006d2ea4 = ADODB.Parameter
                        if (s_allowDebugOverridePolicy)
                            if (string.Equals(clsid.ToString(), "0000050b-0000-0010-8000-00aa006d2ea4", StringComparison.OrdinalIgnoreCase))
                // Hook for testability. IsClassInApprovedList is only called when the system is in global lockdown mode,
                // so this wouldn't be allowed in regular ConstrainedLanguage mode.
                // f6d90f11-9c73-11d3-b32e-00c04f990bb4 = MSXML2.DOMDocument
                if (string.Equals(clsid.ToString(), "f6d90f11-9c73-11d3-b32e-00c04f990bb4", StringComparison.OrdinalIgnoreCase))
        private static SystemEnforcementMode GetLockdownPolicyForResult(uint pdwLockdownState)
            if ((pdwLockdownState & WldpNativeConstants.WLDP_LOCKDOWN_UMCIAUDIT_FLAG) ==
                SystemPolicy.WldpNativeConstants.WLDP_LOCKDOWN_UMCIAUDIT_FLAG)
                return SystemEnforcementMode.Audit;
            else if ((pdwLockdownState & WldpNativeConstants.WLDP_LOCKDOWN_UMCIENFORCE_FLAG) ==
                WldpNativeConstants.WLDP_LOCKDOWN_UMCIENFORCE_FLAG)
        internal static string DumpLockdownState(uint pdwLockdownState)
            if ((pdwLockdownState & WldpNativeConstants.WLDP_LOCKDOWN_DEFINED_FLAG) == WldpNativeConstants.WLDP_LOCKDOWN_DEFINED_FLAG)
                returnValue += "WLDP_LOCKDOWN_DEFINED_FLAG\r\n";
            if ((pdwLockdownState & WldpNativeConstants.WLDP_LOCKDOWN_SECUREBOOT_FLAG) == WldpNativeConstants.WLDP_LOCKDOWN_SECUREBOOT_FLAG)
                returnValue += "WLDP_LOCKDOWN_SECUREBOOT_FLAG\r\n";
            if ((pdwLockdownState & WldpNativeConstants.WLDP_LOCKDOWN_DEBUGPOLICY_FLAG) == WldpNativeConstants.WLDP_LOCKDOWN_DEBUGPOLICY_FLAG)
                returnValue += "WLDP_LOCKDOWN_DEBUGPOLICY_FLAG\r\n";
            if ((pdwLockdownState & WldpNativeConstants.WLDP_LOCKDOWN_UMCIENFORCE_FLAG) == WldpNativeConstants.WLDP_LOCKDOWN_UMCIENFORCE_FLAG)
                returnValue += "WLDP_LOCKDOWN_UMCIENFORCE_FLAG\r\n";
            if ((pdwLockdownState & WldpNativeConstants.WLDP_LOCKDOWN_UMCIAUDIT_FLAG) == WldpNativeConstants.WLDP_LOCKDOWN_UMCIAUDIT_FLAG)
                returnValue += "WLDP_LOCKDOWN_UMCIAUDIT_FLAG\r\n";
        // Overrides for features that should only be enabled in debug mode
        internal static bool XamlWorkflowSupported { get; set; }
        /// Native constants for dealing with the lockdown policy.
        internal static class WldpNativeConstants
            internal const uint WLDP_HOST_INFORMATION_REVISION = 0x00000001;
            internal const uint WLDP_LOCKDOWN_UNDEFINED = 0;
            internal const uint WLDP_LOCKDOWN_DEFINED_FLAG = 0x80000000;
            internal const uint WLDP_LOCKDOWN_SECUREBOOT_FLAG = 1;
            internal const uint WLDP_LOCKDOWN_DEBUGPOLICY_FLAG = 2;
            internal const uint WLDP_LOCKDOWN_UMCIENFORCE_FLAG = 4;
            internal const uint WLDP_LOCKDOWN_UMCIAUDIT_FLAG = 8;
        /// The different host IDs understood by the lockdown policy.
        internal enum WLDP_HOST_ID
            WLDP_HOST_ID_UNKNOWN = 0,
            WLDP_HOST_ID_GLOBAL = 1,
            WLDP_HOST_ID_VBA = 2,
            WLDP_HOST_ID_WSH = 3,
            WLDP_HOST_ID_POWERSHELL = 4,
            WLDP_HOST_ID_IE = 5,
            WLDP_HOST_ID_MSI = 6,
            WLDP_HOST_ID_MAX = 7,
        /// Host information structure to contain the lockdown policy request.
        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct WLDP_HOST_INFORMATION
            internal uint dwRevision;
            /// WLDP_HOST_ID->_WLDP_HOST_ID
            internal WLDP_HOST_ID dwHostId;
            /// PCWSTR->WCHAR*
            [MarshalAsAttribute(UnmanagedType.LPWStr)]
            internal string szSource;
            // HANDLE->IntPtr
            internal IntPtr hSource;
        /// Options for WldpCanExecuteFile method.
        internal enum WLDP_EXECUTION_EVALUATION_OPTIONS
            WLDP_EXECUTION_EVALUATION_OPTION_NONE = 0x0,
            WLDP_EXECUTION_EVALUATION_OPTION_EXECUTE_IN_INTERACTIVE_SESSION = 0x1
        /// Results from WldpCanExecuteFile method.
        internal enum WLDP_EXECUTION_POLICY
            WLDP_CAN_EXECUTE_BLOCKED = 0,
            WLDP_CAN_EXECUTE_ALLOWED = 1,
            WLDP_CAN_EXECUTE_REQUIRE_SANDBOX = 2
        /// Powershell Script Host.
        internal static readonly Guid PowerShellHost = new Guid("8E9AAA7C-198B-4879-AE41-A50D47AD6458");
        /// Native methods for dealing with the lockdown policy.
        internal static class WldpNativeMethods
            /// Returns a WLDP_EXECUTION_POLICY enum value indicating if and how a script file
            /// should be executed.
            /// <param name="host">Host guid.</param>
            /// <param name="options">Evaluation options.</param>
            /// <param name="fileHandle">Evaluated file handle.</param>
            /// <param name="auditInfo">Auditing information string.</param>
            /// <param name="result">Evaluation result.</param>
            /// <returns>HResult value.</returns>
            [DefaultDllImportSearchPathsAttribute(DllImportSearchPath.System32)]
            [DllImportAttribute("wldp.dll", EntryPoint = "WldpCanExecuteFile")]
            internal static extern int WldpCanExecuteFile(
                [MarshalAs(UnmanagedType.LPStruct)]
                Guid host,
                WLDP_EXECUTION_EVALUATION_OPTIONS options,
                IntPtr fileHandle,
                string auditInfo,
                out WLDP_EXECUTION_POLICY result);
            /// pHostInformation: PWLDP_HOST_INFORMATION->_WLDP_HOST_INFORMATION*
            /// pdwLockdownState: PDWORD->DWORD*
            /// dwFlags: DWORD->unsigned int
            [DllImportAttribute("wldp.dll", EntryPoint = "WldpGetLockdownPolicy")]
            internal static extern int WldpGetLockdownPolicy(
                ref WLDP_HOST_INFORMATION pHostInformation,
                ref uint pdwLockdownState,
            /// rclsid: IID*
            /// ptIsApproved: PBOOL->BOOL*
            [DllImportAttribute("wldp.dll", EntryPoint = "WldpIsClassInApprovedList")]
            internal static extern int WldpIsClassInApprovedList(
                ref Guid rclsid,
                ref int ptIsApproved,
            [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern int SHGetKnownFolderPath(
                Guid rfid,
                int dwFlags,
                IntPtr hToken,
                out IntPtr pszPath);

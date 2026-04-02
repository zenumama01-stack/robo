    /// This enum is used to distinguish two sets of parameters on some of the remoting cmdlets.
    internal enum RunspaceParameterSet
        /// Use ComputerName parameter set.
        ComputerName,
        /// Use Runspace Parameter set.
        Runspace
    /// This is a static utility class that performs some of the common chore work for the
    /// the remoting cmdlets.
    internal static class RemotingCommandUtil
        internal static bool HasRepeatingRunspaces(PSSession[] runspaceInfos)
            if (runspaceInfos == null)
                throw PSTraceSource.NewArgumentNullException(nameof(runspaceInfos));
            if (runspaceInfos.GetLength(0) == 0)
                throw PSTraceSource.NewArgumentException(nameof(runspaceInfos));
            for (int i = 0; i < runspaceInfos.GetLength(0); i++)
                for (int k = 0; k < runspaceInfos.GetLength(0); k++)
                    if (i != k)
                        if (runspaceInfos[i].Runspace.InstanceId == runspaceInfos[k].Runspace.InstanceId)
        internal static bool ExceedMaximumAllowableRunspaces(PSSession[] runspaceInfos)
        /// Checks the prerequisites for a cmdlet and terminates if the cmdlet
        /// is not valid.
        internal static void CheckRemotingCmdletPrerequisites()
            // TODO: check that PSRP requirements are installed
            bool notSupported = true;
            const string WSManKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\WSMAN\\";
            CheckHostRemotingPrerequisites();
                // the following registry key defines WSMan compatibility
                // HKLM\Software\Microsoft\Windows\CurrentVersion\WSMAN\ServiceStackVersion
                string wsManStackValue = null;
                RegistryKey wsManKey = Registry.LocalMachine.OpenSubKey(WSManKeyPath);
                if (wsManKey != null)
                    wsManStackValue = (string)wsManKey.GetValue("ServiceStackVersion");
                Version wsManStackVersion = !string.IsNullOrEmpty(wsManStackValue) ?
                    new Version(wsManStackValue.Trim()) :
                    System.Management.Automation.Remoting.Client.WSManNativeApi.WSMAN_STACK_VERSION;
                // WSMan stack version must be 2.0 or later.
                if (wsManStackVersion >= new Version(2, 0))
                    notSupported = false;
                notSupported = true;
            if (notSupported)
                // WSMan is not supported on this platform
                     "Windows PowerShell remoting features are not enabled or not supported on this machine.\nThis may be because you do not have the correct version of WS-Management installed or this version of Windows does not support remoting currently.\n For more information, type 'get-help about_remote_requirements'.");
        /// Facilitates to check if remoting is supported on the host machine.
        /// PowerShell remoting is supported on all Windows SQU's except WinPE.
        /// When PowerShell is hosted on a WinPE machine, the execution
        /// of this API would result in an InvalidOperationException being
        /// thrown, indicating that remoting is not supported on a WinPE machine.
        internal static void CheckHostRemotingPrerequisites()
            // A registry key indicates if the SKU is WINPE. If this turns out to be true,
            // then an InValidOperation exception is thrown.
            bool isWinPEHost = Utils.IsWinPEHost();
            if (isWinPEHost)
                // throw new InvalidOperationException(
                //     "WinPE does not support Windows PowerShell remoting");
                ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.WinPERemotingNotSupported)), null, ErrorCategory.InvalidOperation, null);

    /// Defines the authorization policy that controls the way scripts
    /// (and other command types) are handled by PowerShell.  This authorization
    /// policy enforces one of four levels, as defined by the 'ExecutionPolicy'
    /// value in one of the following locations:
    ///    HKLM\Software\Policies\Microsoft\Windows\PowerShell
    ///    HKCU\Software\Policies\Microsoft\Windows\PowerShell.
    ///    ENV:PSExecutionPolicyPreference
    ///    HKEY_CURRENT_USER\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell
    ///    - Local machine preference
    ///    HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell
    /// Restricted - All .ps1 files are blocked.  ps1xml files must be digitally
    ///    signed, and by a trusted publisher.  If you haven't made a trust decision
    ///    on the publisher yet, prompting is done as in AllSigned mode.
    /// AllSigned - All .ps1 and .ps1xml files must be digitally signed.  If
    ///    signed and executed, PowerShell prompts to determine if files from the
    ///    signing publisher should be run or not.
    /// RemoteSigned - Only .ps1 and .ps1xml files originating from the internet
    ///    must be digitally signed.  If remote, signed, and executed, PowerShell
    ///    prompts to determine if files from the signing publisher should be
    ///    run or not.  This is the default setting.
    /// Unrestricted - No files must be signed.  If a file originates from the
    ///    internet, PowerShell provides a warning prompt to alert the user.  To
    ///    suppress this warning message, right-click on the file in File Explorer,
    ///    select "Properties," and then "Unblock."  Requires Shell.
    /// Bypass - No files must be signed, and internet origin is not verified.
    public sealed class PSAuthorizationManager : AuthorizationManager
        internal enum RunPromptDecision
            NeverRun = 0,
            DoNotRun = 1,
            RunOnce = 2,
            AlwaysRun = 3,
            Suspend = 4
        // execution policy that dictates what can run in msh
        // shellId supplied by runspace configuration
        private readonly string _shellId;
        /// Initializes a new instance of the PSAuthorizationManager
        /// class, for a given ShellId.
        /// The shell identifier that the authorization manager applies
        /// to.  For example, Microsoft.PowerShell
        public PSAuthorizationManager(string shellId)
            : base(shellId)
            if (string.IsNullOrEmpty(shellId))
                throw PSTraceSource.NewArgumentNullException(nameof(shellId));
            _shellId = shellId;
        #region signing check
        private static bool IsSupportedExtension(string ext)
                       ext.Equals(".ps1", StringComparison.OrdinalIgnoreCase) ||
                       ext.Equals(".ps1xml", StringComparison.OrdinalIgnoreCase) ||
                       ext.Equals(".psm1", StringComparison.OrdinalIgnoreCase) ||
                       ext.Equals(".psd1", StringComparison.OrdinalIgnoreCase) ||
                       ext.Equals(".xaml", StringComparison.OrdinalIgnoreCase) ||
                       ext.Equals(".cdxml", StringComparison.OrdinalIgnoreCase));
        private bool CheckPolicy(ExternalScriptInfo script, PSHost host, out Exception reason)
            bool policyCheckPassed = false;
            string path = script.Path;
            string reasonMessage;
            // path is assumed to be fully qualified here
            if (path.IndexOf(System.IO.Path.DirectorySeparatorChar) < 0)
                throw PSTraceSource.NewArgumentException("path");
            if (path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) == (path.Length - 1))
            FileInfo fi = new FileInfo(path);
            // Return false if the file does not exist, so that
            // we don't introduce a race condition
            if (!fi.Exists)
                reason = new FileNotFoundException(path);
            // Quick exit if we don't support the file type
            if (!IsSupportedExtension(fi.Extension))
            // Get the execution policy
            _executionPolicy = SecuritySupport.GetExecutionPolicy(_shellId);
            // Always check the SAFER APIs if code integrity isn't being handled system-wide through
            // WLDP or AppLocker. In those cases, the scripts will be run in ConstrainedLanguage.
            // Otherwise, block.
            // SAFER APIs are not on CSS or OneCore
            if (SystemPolicy.GetSystemLockdownPolicy() != SystemEnforcementMode.Enforce)
                SaferPolicy saferPolicy = SaferPolicy.Disallowed;
                int saferAttempt = 0;
                bool gotSaferPolicy = false;
                // We need to put in a retry workaround, as the SAFER APIs fail when under stress.
                while ((!gotSaferPolicy) && (saferAttempt < 5))
                        saferPolicy = SecuritySupport.GetSaferPolicy(path, null);
                        gotSaferPolicy = true;
                        if (saferAttempt > 4)
                        saferAttempt++;
                // If the script is disallowed via AppLocker, block the file
                // unless the system-wide lockdown policy is "Enforce" (where all PowerShell
                // scripts are in blocked). If the system policy is "Enforce", then the
                // script will be allowed (but ConstrainedLanguage will be applied).
                if (saferPolicy == SaferPolicy.Disallowed)
                    reasonMessage = StringUtil.Format(Authenticode.Reason_DisallowedBySafer, path);
                    reason = new UnauthorizedAccessException(reasonMessage);
            // WLDP and Applocker takes priority over powershell execution policy.
            // See if they want to bypass the authorization manager
            if (_executionPolicy == ExecutionPolicy.Bypass)
            if (_executionPolicy == ExecutionPolicy.Unrestricted)
                // Product binaries are always trusted
                // This avoids signature and security zone checks
                if (SecuritySupport.IsProductBinary(path))
                // We need to give the "Remote File" warning
                // if the file originated from the internet
                if (!IsLocalFile(fi.FullName))
                    // Get the signature of the file.
                    if (string.IsNullOrEmpty(script.ScriptContents))
                        reasonMessage = StringUtil.Format(Authenticode.Reason_FileContentUnavailable, path);
                    Signature signature = GetSignatureWithEncodingRetry(path, script);
                    // The file is signed, with a publisher that
                    // we trust
                    if (signature.Status == SignatureStatus.Valid)
                        // The file is signed by a trusted publisher
                        if (IsTrustedPublisher(signature, path))
                            policyCheckPassed = true;
                    // We don't care about the signature.  If you distrust them,
                    // or the signature does not exist, we prompt you only
                    // because it's remote.
                    if (!policyCheckPassed)
                        RunPromptDecision decision = RunPromptDecision.DoNotRun;
                        // Get their remote prompt answer, allowing them to
                        // enter nested prompts, if wanted.
                            decision = RemoteFilePrompt(path, host);
                            if (decision == RunPromptDecision.Suspend)
                                host.EnterNestedPrompt();
                        } while (decision == RunPromptDecision.Suspend);
                        switch (decision)
                            case RunPromptDecision.RunOnce:
                            case RunPromptDecision.DoNotRun:
                                policyCheckPassed = false;
                                reasonMessage = StringUtil.Format(Authenticode.Reason_DoNotRun, path);
            // Don't need to check the signature if the file is local
            // and we're in "RemoteSigned" mode
            else if ((IsLocalFile(fi.FullName)) &&
                    (_executionPolicy == ExecutionPolicy.RemoteSigned))
            else if ((_executionPolicy == ExecutionPolicy.AllSigned) ||
                // if policy requires signature verification,
                // make it so.
                // The file is signed.
                    // The file is signed by an unknown publisher,
                    // So prompt.
                        policyCheckPassed = SetPolicyFromAuthenticodePrompt(path, host, ref reason, signature);
                // The file is UnknownError, NotSigned, HashMismatch,
                // NotTrusted, NotSupportedFileFormat
                    if (signature.Status == SignatureStatus.NotTrusted)
                        reason = new UnauthorizedAccessException(
                            StringUtil.Format(Authenticode.Reason_NotTrusted,
                                signature.SignerCertificate.SubjectName.Name));
                            StringUtil.Format(Authenticode.Reason_Unknown,
                                signature.StatusMessage));
            else // if(executionPolicy == ExecutionPolicy.Restricted)
                // Deny everything
                // But accept mshxml files from publishers that we
                // trust, or files in the system protected directories
                bool reasonSet = false;
                if (string.Equals(fi.Extension, ".ps1xml", StringComparison.OrdinalIgnoreCase))
                    string[] trustedDirectories = new string[]
                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                    foreach (string trustedDirectory in trustedDirectories)
                        if (fi.FullName.StartsWith(trustedDirectory, StringComparison.OrdinalIgnoreCase))
                                reasonSet = true;
                if (!policyCheckPassed && !reasonSet)
                        StringUtil.Format(Authenticode.Reason_RestrictedMode,
            return policyCheckPassed;
        private static bool SetPolicyFromAuthenticodePrompt(string path, PSHost host, ref Exception reason, Signature signature)
            RunPromptDecision decision = AuthenticodePrompt(path, signature, host);
                    policyCheckPassed = true; break;
                case RunPromptDecision.AlwaysRun:
                        TrustPublisher(signature);
                case RunPromptDecision.NeverRun:
                        UntrustPublisher(signature);
                        reasonMessage = StringUtil.Format(Authenticode.Reason_NeverRun, path);
        private static bool IsLocalFile(string filename)
            SecurityZone zone = ClrFacade.GetFileSecurityZone(filename);
            if (zone == SecurityZone.MyComputer ||
                zone == SecurityZone.Intranet ||
                zone == SecurityZone.Trusted)
        // Checks that a publisher is trusted by the system or is one of
        // the signed product binaries
        private static bool IsTrustedPublisher(Signature signature, string file)
            // Get the thumbprint of the current signature
            X509Certificate2 signerCertificate = signature.SignerCertificate;
            string thumbprint = signerCertificate.Thumbprint;
            // See if it matches any in the list of trusted publishers
            X509Store trustedPublishers = new X509Store(StoreName.TrustedPublisher, StoreLocation.CurrentUser);
            trustedPublishers.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 trustedCertificate in trustedPublishers.Certificates)
                if (string.Equals(trustedCertificate.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase))
                    if (!IsUntrustedPublisher(signature, file))
        private static bool IsUntrustedPublisher(Signature signature, string file)
            X509Store trustedPublishers = new X509Store(StoreName.Disallowed, StoreLocation.CurrentUser);
        /// Trust a publisher by adding it to the "Trusted Publishers" store.
        /// <param name="signature"></param>
        private static void TrustPublisher(Signature signature)
            // Get the certificate of the signer
                // Add it to the list of trusted publishers
                trustedPublishers.Open(OpenFlags.ReadWrite);
                trustedPublishers.Add(signerCertificate);
                trustedPublishers.Close();
        private static void UntrustPublisher(Signature signature)
            X509Store untrustedPublishers = new X509Store(StoreName.Disallowed, StoreLocation.CurrentUser);
            // Remove it from the list of trusted publishers
                // Remove the signer, if it's there
                trustedPublishers.Remove(signerCertificate);
                // Add it to the list of untrusted publishers
                untrustedPublishers.Open(OpenFlags.ReadWrite);
                untrustedPublishers.Add(signerCertificate);
                untrustedPublishers.Close();
        // Check the signature via the SIP which should never erroneously validate an invalid signature
        // or altered script.
        private static Signature GetSignatureWithEncodingRetry(string path, ExternalScriptInfo script)
            // Invoke the SIP directly with the most simple method
            Signature signature = SignatureHelper.GetSignature(path, fileContent: null);
            // try harder to validate the signature by being explicit about encoding
            // and providing the script contents
            byte[] bytesWithBom = GetContentBytesWithBom(script.OriginalEncoding, script.ScriptContents);
            signature = SignatureHelper.GetSignature(path, bytesWithBom);
            // A last ditch effort -
            // If the file was originally ASCII or UTF8, the SIP may have added the Unicode BOM
            if (signature.Status != SignatureStatus.Valid
                && script.OriginalEncoding != Encoding.Unicode)
                bytesWithBom = GetContentBytesWithBom(Encoding.Unicode, script.ScriptContents);
                Signature fallbackSignature = SignatureHelper.GetSignature(path, bytesWithBom);
                if (fallbackSignature.Status == SignatureStatus.Valid)
                    signature = fallbackSignature;
        private static byte[] GetContentBytesWithBom(Encoding encoding, string scriptContent)
            ReadOnlySpan<byte> bomBytes = encoding.Preamble;
            byte[] contentBytes = encoding.GetBytes(scriptContent);
            byte[] bytesWithBom = new byte[bomBytes.Length + contentBytes.Length];
            bomBytes.CopyTo(bytesWithBom);
            contentBytes.CopyTo(bytesWithBom, index: bomBytes.Length);
            return bytesWithBom;
        #endregion signing check
        /// Determines if should run the specified command.  Please see the
        /// class summary for an overview of the semantics enforced by this
        /// authorization manager.
        /// The command to be run.
        /// The origin of the command.
        /// The PSHost executing the command.
        /// If access is denied, this parameter provides a specialized
        /// Exception as the reason.
        /// True if the command should be run.  False otherwise.
        /// CommandInfo is invalid. This may occur if
        /// commandInfo.Name is null or empty.
        /// CommandInfo is null.
        /// The file specified by commandInfo.Path is not found.
        protected internal override bool ShouldRun(CommandInfo commandInfo,
            bool allowRun = false;
            Utils.CheckArgForNull(commandInfo, "commandInfo");
            Utils.CheckArgForNullOrEmpty(commandInfo.Name, "commandInfo.Name");
                    // Always allow cmdlets to run
                    allowRun = true;
                    // we do not care about verifying an alias as we will
                    // get subsequent call(s) for commands/scripts
                    // when the alias is expanded.
                    // we do not check functions/filters.
                    // we only perform script level check.
                    // Allow scripts that are built into the
                    // runspace configuration to run.
                    if (commandInfo is not ExternalScriptInfo si)
                        reason = PSTraceSource.NewArgumentException("scriptInfo");
                            ParserEventSource.Log.CheckSecurityStart(si.Path);
                        allowRun = CheckPolicy(si, host, out reason);
                            ParserEventSource.Log.CheckSecurityStop(si.Path);
                    // We do not check executables -- that is done by Windows
            return allowRun;
        private static RunPromptDecision AuthenticodePrompt(string path,
                                                Signature signature,
            if ((host == null) || (host.UI == null))
                return RunPromptDecision.DoNotRun;
            if (signature == null)
                return decision;
            switch (signature.Status)
                // we do not allow execution in any one of the
                // following cases
                    decision = RunPromptDecision.DoNotRun;
                    Collection<ChoiceDescription> choices = GetAuthenticodePromptChoices();
                    string promptCaption =
                        Authenticode.AuthenticodePromptCaption;
                    string promptText;
                    if (signature.SignerCertificate == null)
                        promptText =
                            StringUtil.Format(Authenticode.AuthenticodePromptText_UnknownPublisher,
                            StringUtil.Format(Authenticode.AuthenticodePromptText,
                                signature.SignerCertificate.SubjectName.Name
                    int userChoice =
                        host.UI.PromptForChoice(promptCaption,
                                                    promptText,
                                                    (int)RunPromptDecision.DoNotRun);
                    decision = (RunPromptDecision)userChoice;
                // if the publisher is not trusted, we prompt and
                // ask the user if s/he wants to allow it to run
        private static RunPromptDecision RemoteFilePrompt(string path, PSHost host)
            Collection<ChoiceDescription> choices = GetRemoteFilePromptChoices();
                Authenticode.RemoteFilePromptCaption;
            string promptText =
                    StringUtil.Format(Authenticode.RemoteFilePromptText,
            int userChoice = host.UI.PromptForChoice(promptCaption,
            switch (userChoice)
                case 0: return RunPromptDecision.DoNotRun;
                case 1: return RunPromptDecision.RunOnce;
                case 2: return RunPromptDecision.Suspend;
                default: return RunPromptDecision.DoNotRun;
        private static Collection<ChoiceDescription> GetAuthenticodePromptChoices()
            string neverRun = Authenticode.Choice_NeverRun;
            string neverRunHelp = Authenticode.Choice_NeverRun_Help;
            string doNotRun = Authenticode.Choice_DoNotRun;
            string doNotRunHelp = Authenticode.Choice_DoNotRun_Help;
            string runOnce = Authenticode.Choice_RunOnce;
            string runOnceHelp = Authenticode.Choice_RunOnce_Help;
            string alwaysRun = Authenticode.Choice_AlwaysRun;
            string alwaysRunHelp = Authenticode.Choice_AlwaysRun_Help;
            choices.Add(new ChoiceDescription(neverRun, neverRunHelp));
            choices.Add(new ChoiceDescription(doNotRun, doNotRunHelp));
            choices.Add(new ChoiceDescription(runOnce, runOnceHelp));
            choices.Add(new ChoiceDescription(alwaysRun, alwaysRunHelp));
            return choices;
        private static Collection<ChoiceDescription> GetRemoteFilePromptChoices()
            string suspend = Authenticode.Choice_Suspend;
            string suspendHelp = Authenticode.Choice_Suspend_Help;
            choices.Add(new ChoiceDescription(suspend, suspendHelp));

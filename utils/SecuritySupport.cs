    /// Defines the different Execution Policies supported by the
    /// PSAuthorizationManager class.
    public enum ExecutionPolicy
        ///    select "Properties," and then "Unblock."
        Unrestricted = 0,
        RemoteSigned = 1,
        AllSigned = 2,
        /// Restricted - All .ps1 files are blocked.  Ps1xml files must be digitally
        Restricted = 3,
        /// Bypass - No files must be signed, and internet origin is not verified
        Bypass = 4,
        /// Undefined - Not specified at this scope
        Undefined = 5,
        /// Default - The most restrictive policy available.
        Default = Restricted
    /// Defines the available configuration scopes for an execution
    /// policy. They are in the following priority, with successive
    /// elements overriding the items that precede them:
    /// LocalMachine -> CurrentUser -> Runspace.
    public enum ExecutionPolicyScope
        /// Execution policy is retrieved from the
        /// PSExecutionPolicyPreference environment variable.
        Process = 0,
        /// Execution policy is retrieved from the HKEY_CURRENT_USER
        /// registry hive for the current ShellId.
        CurrentUser = 1,
        /// Execution policy is retrieved from the HKEY_LOCAL_MACHINE
        LocalMachine = 2,
        /// Execution policy is retrieved from the current user's
        /// group policy setting.
        UserPolicy = 3,
        /// Execution policy is retrieved from the machine-wide
        MachinePolicy = 4
    /// The SAFER policy associated with this file.
    internal enum SaferPolicy
        /// Explicitly allowed through an Allow rule
        ExplicitlyAllowed = 0,
        /// Allowed because it has not been explicitly disallowed
        Allowed = 1,
        /// Disallowed by a rule or policy.
        Disallowed = 2
    /// Security Support APIs.
    public static class SecuritySupport
        #region execution policy
        internal static ExecutionPolicyScope[] ExecutionPolicyScopePreferences
                return new ExecutionPolicyScope[] {
                        ExecutionPolicyScope.MachinePolicy,
                        ExecutionPolicyScope.UserPolicy,
                        ExecutionPolicyScope.Process,
                        ExecutionPolicyScope.CurrentUser,
                        ExecutionPolicyScope.LocalMachine
        internal static void SetExecutionPolicy(ExecutionPolicyScope scope, ExecutionPolicy policy, string shellId)
            string executionPolicy = "Restricted";
            switch (policy)
                case ExecutionPolicy.Restricted:
                    executionPolicy = "Restricted";
                case ExecutionPolicy.AllSigned:
                    executionPolicy = "AllSigned";
                case ExecutionPolicy.RemoteSigned:
                    executionPolicy = "RemoteSigned";
                case ExecutionPolicy.Unrestricted:
                    executionPolicy = "Unrestricted";
                case ExecutionPolicy.Bypass:
                    executionPolicy = "Bypass";
            switch (scope)
                case ExecutionPolicyScope.Process:
                    if (policy == ExecutionPolicy.Undefined)
                        executionPolicy = null;
                    Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", executionPolicy);
                case ExecutionPolicyScope.CurrentUser:
                    // They want to remove it
                        PowerShellConfig.Instance.RemoveExecutionPolicy(ConfigScope.CurrentUser, shellId);
                        PowerShellConfig.Instance.SetExecutionPolicy(ConfigScope.CurrentUser, shellId, executionPolicy);
                case ExecutionPolicyScope.LocalMachine:
                        PowerShellConfig.Instance.RemoveExecutionPolicy(ConfigScope.AllUsers, shellId);
                        PowerShellConfig.Instance.SetExecutionPolicy(ConfigScope.AllUsers, shellId, executionPolicy);
        internal static ExecutionPolicy GetExecutionPolicy(string shellId)
            foreach (ExecutionPolicyScope scope in ExecutionPolicyScopePreferences)
                ExecutionPolicy policy = GetExecutionPolicy(shellId, scope);
                if (policy != ExecutionPolicy.Undefined)
            return ExecutionPolicy.Restricted;
        private static bool? _hasGpScriptParent;
        /// A value indicating that the current process was launched by GPScript.exe
        /// Used to determine execution policy when group policies are in effect.
        /// This is somewhat expensive to determine and does not change within the lifetime of the current process
        private static bool HasGpScriptParent
                if (!_hasGpScriptParent.HasValue)
                    _hasGpScriptParent = IsCurrentProcessLaunchedByGpScript();
                return _hasGpScriptParent.Value;
        private static bool IsCurrentProcessLaunchedByGpScript()
            Process currentProcess = Process.GetCurrentProcess();
            string gpScriptPath = IO.Path.Combine(
                "gpscript.exe");
            bool foundGpScriptParent = false;
                while (currentProcess != null)
                    if (string.Equals(gpScriptPath,
                            currentProcess.MainModule.FileName, StringComparison.OrdinalIgnoreCase))
                        foundGpScriptParent = true;
                        currentProcess = PsUtils.GetParentProcess(currentProcess);
                // If you attempt to retrieve the MainModule of a 64-bit process
                // from a WOW64 (32-bit) process, the Win32 API has a fatal
                // flaw that causes this to return the error:
                //   "Only part of a ReadProcessMemory or WriteProcessMemory
                //   request was completed."
                // In this case, we just catch the exception and eat it.
                // The implication is that logon / logoff scripts that somehow
                // launch the Wow64 version of PowerShell will be subject
                // to the execution policy deployed by Group Policy (where
                // our goal here is to not have the Group Policy execution policy
                // affect logon / logoff scripts.
            return foundGpScriptParent;
        internal static ExecutionPolicy GetExecutionPolicy(string shellId, ExecutionPolicyScope scope)
            return ExecutionPolicy.Unrestricted;
                        string policy = Environment.GetEnvironmentVariable("PSExecutionPolicyPreference");
                        if (!string.IsNullOrEmpty(policy))
                            return ParseExecutionPolicy(policy);
                            return ExecutionPolicy.Undefined;
                        string policy = GetLocalPreferenceValue(shellId, scope);
                // TODO: Group Policy is only supported on Full systems, but !LINUX && CORECLR
                // will run there as well, so I don't think we should remove it.
                case ExecutionPolicyScope.UserPolicy:
                case ExecutionPolicyScope.MachinePolicy:
                        string groupPolicyPreference = GetGroupPolicyValue(shellId, scope);
                        // Be sure we aren't being called by Group Policy
                        // itself. A group policy should never block a logon /
                        // logoff script.
                        if (string.IsNullOrEmpty(groupPolicyPreference) || HasGpScriptParent)
                        return ParseExecutionPolicy(groupPolicyPreference);
        internal static ExecutionPolicy ParseExecutionPolicy(string policy)
            if (string.Equals(policy, "Bypass",
                return ExecutionPolicy.Bypass;
            else if (string.Equals(policy, "Unrestricted",
            else if (string.Equals(policy, "RemoteSigned",
                return ExecutionPolicy.RemoteSigned;
            else if (string.Equals(policy, "AllSigned",
                return ExecutionPolicy.AllSigned;
            else if (string.Equals(policy, "Restricted",
                return ExecutionPolicy.Default;
        internal static string GetExecutionPolicy(ExecutionPolicy policy)
                    return "Bypass";
                    return "Unrestricted";
                    return "RemoteSigned";
                    return "AllSigned";
                    return "Restricted";
        /// Returns true if file has product binary signature.
        /// <param name="file">Name of file to check.</param>
        /// <returns>True when file has product binary signature.</returns>
        public static bool IsProductBinary(string file)
            if (string.IsNullOrEmpty(file) || (!IO.File.Exists(file)))
            // Check if it is in the product folder, if not, skip checking the catalog
            // and any other checks.
            var isUnderProductFolder = Utils.IsUnderProductFolder(file);
            if (!isUnderProductFolder)
            // There is no signature support on non-Windows platforms (yet), when
            // execution reaches here, we are sure the file is under product folder
            // Check the file signature
            Signature fileSignature = SignatureHelper.GetSignature(file, null);
            if ((fileSignature != null) && (fileSignature.IsOSBinary))
            // WTGetSignatureInfo, via Microsoft.Security.Extensions, is used to verify catalog signature.
            // On Win7, catalog API is not available.
            // On OneCore SKUs like NanoServer/IoT, the API has a bug that makes it not able to find the
            // corresponding catalog file for a given product file, so it doesn't work properly.
            // In these cases, we just trust the 'isUnderProductFolder' check.
                // When execution reaches here, we are sure the file is under product folder
        /// Returns the value of the Execution Policy as retrieved
        /// from group policy.
        /// <returns>NULL if it is not defined at this level.</returns>
        private static string GetGroupPolicyValue(string shellId, ExecutionPolicyScope scope)
            ConfigScope[] scopeKey = null;
                    scopeKey = Utils.SystemWideOnlyConfig;
                    scopeKey = Utils.CurrentUserOnlyConfig;
            var scriptExecutionSetting = Utils.GetPolicySetting<ScriptExecution>(scopeKey);
            if (scriptExecutionSetting != null)
                if (scriptExecutionSetting.EnableScripts == false)
                    // Script execution is explicitly disabled
                else if (scriptExecutionSetting.EnableScripts == true)
                    // Script execution is explicitly enabled
                    return scriptExecutionSetting.ExecutionPolicy;
        /// from the local preference.
        private static string GetLocalPreferenceValue(string shellId, ExecutionPolicyScope scope)
                // 1: Look up the current-user preference
                    return PowerShellConfig.Instance.GetExecutionPolicy(ConfigScope.CurrentUser, shellId);
                // 2: Look up the system-wide preference
                    return PowerShellConfig.Instance.GetExecutionPolicy(ConfigScope.AllUsers, shellId);
        #endregion execution policy
        private static bool _saferIdentifyLevelApiSupported = true;
        /// Get the pass / fail result of calling the SAFER API.
        /// <param name="path">The path to the file in question.</param>
        /// <param name="handle">A file handle to the file in question, if available.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        internal static SaferPolicy GetSaferPolicy(string path, SafeHandle handle)
            SaferPolicy status = SaferPolicy.Allowed;
            if (!_saferIdentifyLevelApiSupported)
            SAFER_CODE_PROPERTIES codeProperties = new SAFER_CODE_PROPERTIES();
            IntPtr hAuthzLevel;
            // Prepare the code properties struct.
            codeProperties.cbSize = (uint)Marshal.SizeOf(typeof(SAFER_CODE_PROPERTIES));
            codeProperties.dwCheckFlags = (
                NativeConstants.SAFER_CRITERIA_IMAGEPATH |
                NativeConstants.SAFER_CRITERIA_IMAGEHASH |
                NativeConstants.SAFER_CRITERIA_AUTHENTICODE);
            codeProperties.ImagePath = path;
            if (handle != null)
                codeProperties.hImageFileHandle = handle.DangerousGetHandle();
            // turn off WinVerifyTrust UI
            codeProperties.dwWVTUIChoice = NativeConstants.WTD_UI_NONE;
            // Identify the level associated with the code
            if (NativeMethods.SaferIdentifyLevel(1, ref codeProperties, out hAuthzLevel, NativeConstants.SRP_POLICY_SCRIPT))
                // We found an Authorization Level applicable to this application.
                IntPtr hRestrictedToken = IntPtr.Zero;
                    if (!NativeMethods.SaferComputeTokenFromLevel(
                                               hAuthzLevel,                    // Safer Level
                                               IntPtr.Zero,                    // Test current process' token
                                               ref hRestrictedToken,           // target token
                                               NativeConstants.SAFER_TOKEN_NULL_IF_EQUAL,
                        if ((lastError == NativeConstants.ERROR_ACCESS_DISABLED_BY_POLICY) ||
                            (lastError == NativeConstants.ERROR_ACCESS_DISABLED_NO_SAFER_UI_BY_POLICY))
                            status = SaferPolicy.Disallowed;
                            throw new System.ComponentModel.Win32Exception();
                        if (hRestrictedToken == IntPtr.Zero)
                            // This is not necessarily the "fully trusted" level,
                            // it means that the thread token is complies with the requested level
                            status = SaferPolicy.Allowed;
                            NativeMethods.CloseHandle(hRestrictedToken);
                    NativeMethods.SaferCloseLevel(hAuthzLevel);
                if (lastError == NativeConstants.FUNCTION_NOT_SUPPORTED)
                    _saferIdentifyLevelApiSupported = false;
                    throw new System.ComponentModel.Win32Exception(lastError);
        /// Throw if file does not exist.
        internal static void CheckIfFileExists(string filePath)
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);
        /// Check to see if the specified cert is suitable to be
        /// used as a code signing cert.
        /// <param name="c">Certificate object.</param>
        internal static bool CertIsGoodForSigning(X509Certificate2 c)
            if (!c.HasPrivateKey)
            return CertHasOid(c, CertificateFilterInfo.CodeSigningOid);
        /// used as an encryption cert for PKI encryption. Note
        /// that this cert doesn't require the private key.
        internal static bool CertIsGoodForEncryption(X509Certificate2 c)
                CertHasOid(c, CertificateFilterInfo.DocumentEncryptionOid) &&
                (CertHasKeyUsage(c, X509KeyUsageFlags.DataEncipherment) ||
                 CertHasKeyUsage(c, X509KeyUsageFlags.KeyEncipherment)));
        /// Check to see if the specified cert is expiring by the time.
        /// <param name="expiring">Certificate expire time.</param>
        internal static bool CertExpiresByTime(X509Certificate2 c, DateTime expiring)
            return c.NotAfter < expiring;
        private static bool CertHasOid(X509Certificate2 c, string oid)
            foreach (var extension in c.Extensions)
                if (extension is X509EnhancedKeyUsageExtension ext)
                    foreach (Oid ekuOid in ext.EnhancedKeyUsages)
                        if (ekuOid.Value == oid)
        private static bool CertHasKeyUsage(X509Certificate2 c, X509KeyUsageFlags keyUsage)
            foreach (X509Extension extension in c.Extensions)
                if (extension is X509KeyUsageExtension keyUsageExtension)
                    if ((keyUsageExtension.KeyUsages & keyUsage) == keyUsage)
        /// Get the EKUs of a cert.
        /// <returns>A collection of cert eku strings.</returns>
        internal static Collection<string> GetCertEKU(X509Certificate2 cert)
            Collection<string> ekus = new Collection<string>();
            IntPtr pCert = cert.Handle;
            int structSize = 0;
            IntPtr dummy = IntPtr.Zero;
            if (Security.NativeMethods.CertGetEnhancedKeyUsage(pCert, 0, dummy,
                                                      out structSize))
                if (structSize > 0)
                    IntPtr ekuBuffer = Marshal.AllocHGlobal(structSize);
                        if (Security.NativeMethods.CertGetEnhancedKeyUsage(pCert, 0,
                                                                  ekuBuffer,
                            Security.NativeMethods.CERT_ENHKEY_USAGE ekuStruct =
                                Marshal.PtrToStructure<Security.NativeMethods.CERT_ENHKEY_USAGE>(ekuBuffer);
                            IntPtr ep = ekuStruct.rgpszUsageIdentifier;
                            IntPtr ekuptr;
                            for (int i = 0; i < ekuStruct.cUsageIdentifier; i++)
                                ekuptr = Marshal.ReadIntPtr(ep, i * Marshal.SizeOf(ep));
                                string eku = Marshal.PtrToStringAnsi(ekuptr);
                                ekus.Add(eku);
                        Marshal.FreeHGlobal(ekuBuffer);
            return ekus;
        /// Convert an int to a DWORD.
        /// <param name="n">Signed int number.</param>
        /// <returns>DWORD.</returns>
        internal static DWORD GetDWORDFromInt(int n)
            UInt32 result = BitConverter.ToUInt32(BitConverter.GetBytes(n), 0);
            return (DWORD)result;
        /// Convert a DWORD to int.
        /// <param name="n">Number.</param>
        /// <returns>Int.</returns>
        internal static int GetIntFromDWORD(DWORD n)
            Int64 n64 = n - 0x100000000L;
            return (int)n64;
    /// Information used for filtering a set of certs.
    internal sealed class CertificateFilterInfo
        internal CertificateFilterInfo()
        /// Gets or sets purpose of a certificate.
        internal CertificatePurpose Purpose
        } = CertificatePurpose.NotSpecified;
        /// Gets or sets SSL Server Authentication.
        internal bool SSLServerAuthentication
        /// Gets or sets DNS name of a certificate.
        internal WildcardPattern DnsName
        /// Gets or sets EKU OID list of a certificate.
        internal List<WildcardPattern> Eku
        /// Gets or sets validity time for a certificate.
        internal DateTime Expiring
        } = DateTime.MinValue;
        internal const string CodeSigningOid = "1.3.6.1.5.5.7.3.3";
        internal const string OID_PKIX_KP_SERVER_AUTH = "1.3.6.1.5.5.7.3.1";
        // The OID arc 1.3.6.1.4.1.311.80 is assigned to PowerShell. If we need
        // new OIDs, we can assign them under this branch.
        internal const string DocumentEncryptionOid = "1.3.6.1.4.1.311.80.1";
        internal const string SubjectAlternativeNameOid = "2.5.29.17";
    /// Defines the valid purposes by which
    /// we can filter certificates.
    internal enum CertificatePurpose
        /// Certificates where a purpose has not been specified.
        /// Certificates that can be used to sign
        /// code and scripts.
        CodeSigning = 0x1,
        /// Certificates that can be used to encrypt
        /// data.
        DocumentEncryption = 0x2,
        /// Certificates that can be used for any
        /// purpose.
        All = 0xffff
    /// Utility class for CMS (Cryptographic Message Syntax) related operations.
    internal static class CmsUtils
        internal static string Encrypt(byte[] contentBytes, CmsMessageRecipient[] recipients, SessionState sessionState, out ErrorRecord error)
            if ((contentBytes == null) || (contentBytes.Length == 0))
            // After review with the crypto board, NIST_AES256_CBC is more appropriate
            // than .NET's default 3DES. Also, when specified, uses szOID_RSAES_OAEP for key
            // encryption to prevent padding attacks.
            const string szOID_NIST_AES256_CBC = "2.16.840.1.101.3.4.1.42";
            ContentInfo content = new ContentInfo(contentBytes);
            EnvelopedCms cms = new EnvelopedCms(content,
                new AlgorithmIdentifier(
                    Oid.FromOidValue(szOID_NIST_AES256_CBC, OidGroup.EncryptionAlgorithm)));
            CmsRecipientCollection recipientCollection = new CmsRecipientCollection();
            foreach (CmsMessageRecipient recipient in recipients)
                // Resolve the recipient, if it hasn't been done yet.
                if ((recipient.Certificates != null) && (recipient.Certificates.Count == 0))
                    recipientCollection.Add(new CmsRecipient(certificate));
            cms.Encrypt(recipientCollection);
            byte[] encodedBytes = cms.Encode();
            string encodedContent = CmsUtils.GetAsciiArmor(encodedBytes);
            return encodedContent;
        internal static readonly string BEGIN_CMS_SIGIL = "-----BEGIN CMS-----";
        internal static readonly string END_CMS_SIGIL = "-----END CMS-----";
        internal static readonly string BEGIN_CERTIFICATE_SIGIL = "-----BEGIN CERTIFICATE-----";
        internal static readonly string END_CERTIFICATE_SIGIL = "-----END CERTIFICATE-----";
        /// Adds Ascii armour to a byte stream in Base64 format.
        /// <param name="bytes">The bytes to encode.</param>
        internal static string GetAsciiArmor(byte[] bytes)
            StringBuilder output = new StringBuilder();
            output.AppendLine(BEGIN_CMS_SIGIL);
            string encodedString = Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
            output.AppendLine(encodedString);
            output.Append(END_CMS_SIGIL);
        /// Removes Ascii armour from a byte stream.
        /// <param name="actualContent">The Ascii armored content.</param>
        /// <param name="beginMarker">The marker of the start of the Base64 content.</param>
        /// <param name="endMarker">The marker of the end of the Base64 content.</param>
        /// <param name="startIndex">The beginning of where the Ascii armor was detected.</param>
        /// <param name="endIndex">The end of where the Ascii armor was detected.</param>
        internal static byte[] RemoveAsciiArmor(string actualContent, string beginMarker, string endMarker, out int startIndex, out int endIndex)
            byte[] messageBytes = null;
            startIndex = -1;
            endIndex = -1;
            startIndex = actualContent.IndexOf(beginMarker, StringComparison.OrdinalIgnoreCase);
            if (startIndex < 0)
            endIndex = actualContent.IndexOf(endMarker, startIndex, StringComparison.OrdinalIgnoreCase) +
                 endMarker.Length;
            if (endIndex < endMarker.Length)
            int startContent = startIndex + beginMarker.Length;
            int endContent = endIndex - endMarker.Length;
            string encodedContent = actualContent.Substring(startContent, endContent - startContent);
            encodedContent = System.Text.RegularExpressions.Regex.Replace(encodedContent, "\\s", string.Empty);
            messageBytes = Convert.FromBase64String(encodedContent);
            return messageBytes;
    /// Represents a message recipient for the Cms cmdlets.
    public class CmsMessageRecipient
        /// Creates an instance of the CmsMessageRecipient class.
        internal CmsMessageRecipient() { }
        /// <param name="identifier">
        ///     The identifier of the CmsMessageRecipient.
        ///     Can be either:
        ///         - The path to a file containing the certificate
        ///         - The path to a directory containing the certificate
        ///         - The thumbprint of the certificate, used to find the certificate in the certificate store
        ///         - The Subject name of the recipient, used to find the certificate in the certificate store
        public CmsMessageRecipient(string identifier)
            this.Certificates = new X509Certificate2Collection();
        /// <param name="certificate">The certificate to use.</param>
        public CmsMessageRecipient(X509Certificate2 certificate)
            _pendingCertificate = certificate;
        private readonly X509Certificate2 _pendingCertificate;
        /// Gets the certificate associated with this recipient.
        public X509Certificate2Collection Certificates
        /// Resolves the provided identifier into a collection of certificates.
        /// <param name="sessionState">A reference to an instance of Powershell's SessionState class.</param>
        /// <param name="purpose">The purpose for which this identifier is being resolved (Encryption / Decryption.</param>
        /// <param name="error">The error generated (if any) for this resolution.</param>
        public void Resolve(SessionState sessionState, ResolutionPurpose purpose, out ErrorRecord error)
            // Process the certificate if that was supplied exactly
            if (_pendingCertificate != null)
                ProcessResolvedCertificates(
                    purpose,
                    new X509Certificate2Collection(_pendingCertificate),
                if ((error != null) || (Certificates.Count != 0))
            if (_identifier != null)
                // First try to resolve assuming that the cert was Base64 encoded.
                ResolveFromBase64Encoding(purpose, out error);
                // Then try to resolve by path.
                ResolveFromPath(sessionState, purpose, out error);
                // Then by cert store
                ResolveFromStoreById(purpose, out error);
            // Generate an error if no cert was found (and this is an encryption attempt).
            // If it is only decryption, then the system will always look in the 'My' store anyways, so
            // don't generate an error if they used wildcards. If they did not use wildcards,
            // then generate an error because they were expecting something specific.
            if ((purpose == ResolutionPurpose.Encryption) ||
                (!WildcardPattern.ContainsWildcardCharacters(_identifier)))
                        string.Format(CultureInfo.InvariantCulture,
                            SecuritySupportStrings.NoCertificateFound, _identifier)),
                    "NoCertificateFound", ErrorCategory.ObjectNotFound, _identifier);
        private void ResolveFromBase64Encoding(ResolutionPurpose purpose, out ErrorRecord error)
                messageBytes = CmsUtils.RemoveAsciiArmor(_identifier, CmsUtils.BEGIN_CERTIFICATE_SIGIL, CmsUtils.END_CERTIFICATE_SIGIL, out startIndex, out endIndex);
                // Not Base-64 encoded
            // Didn't have the sigil
            if (messageBytes == null)
            var certificatesToProcess = new X509Certificate2Collection();
                X509Certificate2 newCertificate = new X509Certificate2(messageBytes);
                certificatesToProcess.Add(newCertificate);
                // User call-out, catch-all OK
                // Wasn't certificate data
            // Now validate the certificate
            ProcessResolvedCertificates(purpose, certificatesToProcess, out error);
        private void ResolveFromPath(SessionState sessionState, ResolutionPurpose purpose, out ErrorRecord error)
            ProviderInfo pathProvider = null;
                resolvedPaths = sessionState.Path.GetResolvedProviderPathFromPSPath(_identifier, out pathProvider);
                // If we got an ItemNotFound / etc., then this didn't represent a valid path.
            // If we got a resolved path, try to load certs from that path.
            if ((resolvedPaths != null) && (resolvedPaths.Count != 0))
                // Ensure the path is from the file system provider
                if (!string.Equals(pathProvider.Name, "FileSystem", StringComparison.OrdinalIgnoreCase))
                                SecuritySupportStrings.CertificatePathMustBeFileSystemPath, _identifier)),
                        "CertificatePathMustBeFileSystemPath", ErrorCategory.ObjectNotFound, pathProvider);
                // If this is a directory, add all certificates in it. This will be the primary
                // scenario for decryption via Group Protected PFX files
                // (http://social.technet.microsoft.com/wiki/contents/articles/13922.certificate-pfx-export-and-import-using-ad-ds-account-protection.aspx)
                List<string> pathsToAdd = new List<string>();
                List<string> pathsToRemove = new List<string>();
                    if (System.IO.Directory.Exists(resolvedPath))
                        // It would be nice to limit this to *.pfx, *.cer, etc., but
                        // the crypto APIs support extracting certificates from arbitrary file types.
                        pathsToAdd.AddRange(System.IO.Directory.GetFiles(resolvedPath));
                        pathsToRemove.Add(resolvedPath);
                // Update resolved paths
                foreach (string path in pathsToAdd)
                foreach (string path in pathsToRemove)
                    resolvedPaths.Remove(path);
                    X509Certificate2 certificate = null;
                        certificate = new X509Certificate2(path);
                    certificatesToProcess.Add(certificate);
        private void ResolveFromStoreById(ResolutionPurpose purpose, out ErrorRecord error)
            WildcardPattern subjectNamePattern = WildcardPattern.Get(_identifier, WildcardOptions.IgnoreCase);
                using (var storeCU = new X509Store("my", StoreLocation.CurrentUser))
                    storeCU.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection storeCerts = storeCU.Certificates;
                        using (var storeLM = new X509Store("my", StoreLocation.LocalMachine))
                            storeLM.Open(OpenFlags.ReadOnly);
                            storeCerts.AddRange(storeLM.Certificates);
                    certificatesToProcess.AddRange(storeCerts.Find(X509FindType.FindByThumbprint, _identifier, validOnly: false));
                    if (certificatesToProcess.Count == 0)
                        foreach (var cert in storeCerts)
                            if (subjectNamePattern.IsMatch(cert.Subject) || subjectNamePattern.IsMatch(cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false)))
                                certificatesToProcess.Add(cert);
        private void ProcessResolvedCertificates(ResolutionPurpose purpose, X509Certificate2Collection certificatesToProcess, out ErrorRecord error)
            HashSet<string> processedThumbprints = new HashSet<string>();
            foreach (X509Certificate2 certificate in certificatesToProcess)
                if (!SecuritySupport.CertIsGoodForEncryption(certificate))
                    // If they specified a specific cert, generate an error if it isn't good
                    // for encryption.
                    if (!WildcardPattern.ContainsWildcardCharacters(_identifier))
                                    SecuritySupportStrings.CertificateCannotBeUsedForEncryption,
                                    certificate.Thumbprint,
                                    CertificateFilterInfo.DocumentEncryptionOid)),
                            "CertificateCannotBeUsedForEncryption",
                            certificate);
                // When decrypting, only look for certs that have the private key
                if (purpose == ResolutionPurpose.Decryption)
                    if (!certificate.HasPrivateKey)
                if (processedThumbprints.Contains(certificate.Thumbprint))
                    processedThumbprints.Add(certificate.Thumbprint);
                if (purpose == ResolutionPurpose.Encryption)
                    // Only let wildcards expand to one recipient. Otherwise, data
                    // may be encrypted to the wrong person on accident.
                    if (Certificates.Count > 0)
                                    SecuritySupportStrings.IdentifierMustReferenceSingleCertificate,
                                    _identifier,
                                    arg1: "To")),
                            "IdentifierMustReferenceSingleCertificate",
                            ErrorCategory.LimitsExceeded,
                            certificatesToProcess);
                        Certificates.Clear();
    /// Defines the purpose for resolution of a CmsMessageRecipient.
    public enum ResolutionPurpose
        /// This message recipient is intended to be used for message encryption.
        Encryption,
        /// This message recipient is intended to be used for message decryption.
        Decryption
    internal static class AmsiUtils
        static AmsiUtils()
                s_amsiInitFailed = !CheckAmsiInit();
                PSEtwLog.LogAmsiUtilStateEvent("DllNotFoundException", $"{s_amsiContext}-{s_amsiSession}");
                s_amsiInitFailed = true;
            PSEtwLog.LogAmsiUtilStateEvent($"init-{s_amsiInitFailed}", $"{s_amsiContext}-{s_amsiSession}");
        internal static int Init()
            Diagnostics.Assert(s_amsiContext == IntPtr.Zero, "Init should be called just once");
            lock (s_amsiLockObject)
                string appName;
                    appName = string.Concat("PowerShell_", Environment.ProcessPath, "_", PSVersionInfo.ProductVersion);
                    // Fall back to 'Process.ProcessName' in case 'Environment.ProcessPath' throws exception.
                    appName = string.Concat("PowerShell_", currentProcess.ProcessName, ".exe_", PSVersionInfo.ProductVersion);
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                var hr = AmsiNativeMethods.AmsiInitialize(appName, ref s_amsiContext);
        /// Scans a string buffer for malware using the Antimalware Scan Interface (AMSI).
        /// Caller is responsible for calling AmsiCloseSession when a "session" (script)
        /// is complete, and for calling AmsiUninitialize when the runspace is being torn down.
        /// <param name="content">The string to be scanned.</param>
        /// <param name="sourceMetadata">Information about the source (filename, etc.).</param>
        /// <returns>AMSI_RESULT_DETECTED if malware was detected in the sample.</returns>
        internal static AmsiNativeMethods.AMSI_RESULT ScanContent(string content, string sourceMetadata)
            return AmsiNativeMethods.AMSI_RESULT.AMSI_RESULT_NOT_DETECTED;
            return WinScanContent(content, sourceMetadata, warmUp: false);
        internal static AmsiNativeMethods.AMSI_RESULT WinScanContent(
            string content,
            string sourceMetadata,
            bool warmUp)
            if (string.IsNullOrEmpty(sourceMetadata))
                sourceMetadata = string.Empty;
            const string EICAR_STRING = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
            if (InternalTestHooks.UseDebugAmsiImplementation)
                if (content.Contains(EICAR_STRING, StringComparison.Ordinal))
                    return AmsiNativeMethods.AMSI_RESULT.AMSI_RESULT_DETECTED;
            // If we had a previous initialization failure, just return the neutral result.
            if (s_amsiInitFailed)
                PSEtwLog.LogAmsiUtilStateEvent("ScanContent-InitFail", $"{s_amsiContext}-{s_amsiSession}");
                    if (!CheckAmsiInit())
                    if (warmUp)
                        // We are warming up the AMSI component in console startup, and that means we initialize AMSI
                        // and create a AMSI session, but don't really scan anything.
                    AmsiNativeMethods.AMSI_RESULT result = AmsiNativeMethods.AMSI_RESULT.AMSI_RESULT_CLEAN;
                    // Run AMSI content scan
                    int hr;
                        fixed (char* buffer = content)
                            var buffPtr = new IntPtr(buffer);
                            hr = AmsiNativeMethods.AmsiScanBuffer(
                                s_amsiContext,
                                buffPtr,
                                (uint)(content.Length * sizeof(char)),
                                s_amsiSession,
                                ref result);
                    if (!Utils.Succeeded(hr))
                        // If we got a failure, just return the neutral result ("AMSI_RESULT_NOT_DETECTED")
                        PSEtwLog.LogAmsiUtilStateEvent($"AmsiScanBuffer-{hr}", $"{s_amsiContext}-{s_amsiSession}");
        /// <Summary>
        /// Reports provided content to AMSI (Antimalware Scan Interface).
        /// </Summary>
        /// <param name="name">Name of content being reported.</param>
        /// <param name="content">Content being reported.</param>
        /// <returns>True if content was successfully reported.</returns>
        internal static bool ReportContent(
            string content)
            return WinReportContent(name, content);
        private static bool WinReportContent(
            if (string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(content) ||
                s_amsiInitFailed ||
                s_amsiNotifyFailed)
                if (s_amsiNotifyFailed)
                    AmsiNativeMethods.AMSI_RESULT result = AmsiNativeMethods.AMSI_RESULT.AMSI_RESULT_NOT_DETECTED;
                            hr = AmsiNativeMethods.AmsiNotifyOperation(
                                amsiContext: s_amsiContext,
                                buffer: buffPtr,
                                length: (uint)(content.Length * sizeof(char)),
                                contentName: name,
                    if (Utils.Succeeded(hr))
                        if (result == AmsiNativeMethods.AMSI_RESULT.AMSI_RESULT_DETECTED)
                            // If malware is detected, throw to prevent method invoke expression from running.
                            throw new PSSecurityException(ParserStrings.ScriptContainedMaliciousContent);
                    s_amsiNotifyFailed = true;
                catch (System.EntryPointNotFoundException)
        private static bool CheckAmsiInit()
            // Initialize AntiMalware Scan Interface, if not already initialized.
            // If we failed to initialize previously, just return the neutral result ("AMSI_RESULT_NOT_DETECTED")
            if (s_amsiContext == IntPtr.Zero)
                int hr = Init();
            // Initialize the session, if one isn't already started.
            if (s_amsiSession == IntPtr.Zero)
                int hr = AmsiNativeMethods.AmsiOpenSession(s_amsiContext, ref s_amsiSession);
                AmsiInitialized = true;
        internal static void CurrentDomain_ProcessExit(object sender, EventArgs e)
            if (AmsiInitialized && !AmsiUninitializeCalled)
                Uninitialize();
        private static IntPtr s_amsiContext = IntPtr.Zero;
        private static IntPtr s_amsiSession = IntPtr.Zero;
        private static readonly bool s_amsiInitFailed = false;
        private static bool s_amsiNotifyFailed = false;
        private static readonly object s_amsiLockObject = new object();
        /// Reset the AMSI session (used to track related script invocations)
        internal static void CloseSession()
            WinCloseSession();
        internal static void WinCloseSession()
            if (!s_amsiInitFailed)
                if ((s_amsiContext != IntPtr.Zero) && (s_amsiSession != IntPtr.Zero))
                        // Clean up the session if one was open.
                            AmsiNativeMethods.AmsiCloseSession(s_amsiContext, s_amsiSession);
                            s_amsiSession = IntPtr.Zero;
        /// Uninitialize the AMSI interface.
        internal static void Uninitialize()
            WinUninitialize();
        internal static void WinUninitialize()
            AmsiUninitializeCalled = true;
                    if (s_amsiContext != IntPtr.Zero)
                        CloseSession();
                        // Unregister the event handler.
                        AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
                        // Uninitialize the AMSI interface.
                        AmsiCleanedUp = true;
                        AmsiNativeMethods.AmsiUninitialize(s_amsiContext);
                        s_amsiContext = IntPtr.Zero;
        public static bool AmsiUninitializeCalled = false;
        public static bool AmsiInitialized = false;
        public static bool AmsiCleanedUp = false;
        internal static class AmsiNativeMethods
            internal enum AMSI_RESULT
                /// AMSI_RESULT_CLEAN -> 0
                AMSI_RESULT_CLEAN = 0,
                /// AMSI_RESULT_NOT_DETECTED -> 1
                AMSI_RESULT_NOT_DETECTED = 1,
                /// Certain policies set by administrator blocked this content on this machine
                AMSI_RESULT_BLOCKED_BY_ADMIN_BEGIN = 0x4000,
                AMSI_RESULT_BLOCKED_BY_ADMIN_END = 0x4fff,
                /// AMSI_RESULT_DETECTED -> 32768
                AMSI_RESULT_DETECTED = 32768,
            ///appName: LPCWSTR->WCHAR*
            ///amsiContext: HAMSICONTEXT*
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            [DllImport("amsi.dll", EntryPoint = "AmsiInitialize", CallingConvention = CallingConvention.StdCall)]
            internal static extern int AmsiInitialize(
                [In][MarshalAs(UnmanagedType.LPWStr)] string appName, ref System.IntPtr amsiContext);
            /// Return Type: void
            ///amsiContext: HAMSICONTEXT->HAMSICONTEXT__*
            [DllImport("amsi.dll", EntryPoint = "AmsiUninitialize", CallingConvention = CallingConvention.StdCall)]
            internal static extern void AmsiUninitialize(System.IntPtr amsiContext);
            ///amsiSession: HAMSISESSION*
            [DllImport("amsi.dll", EntryPoint = "AmsiOpenSession", CallingConvention = CallingConvention.StdCall)]
            internal static extern int AmsiOpenSession(System.IntPtr amsiContext, ref System.IntPtr amsiSession);
            ///amsiSession: HAMSISESSION->HAMSISESSION__*
            [DllImport("amsi.dll", EntryPoint = "AmsiCloseSession", CallingConvention = CallingConvention.StdCall)]
            internal static extern void AmsiCloseSession(System.IntPtr amsiContext, System.IntPtr amsiSession);
            ///buffer: PVOID->void*
            ///length: ULONG->unsigned int
            ///contentName: LPCWSTR->WCHAR*
            ///result: AMSI_RESULT*
            [DllImport("amsi.dll", EntryPoint = "AmsiScanBuffer", CallingConvention = CallingConvention.StdCall)]
            internal static extern int AmsiScanBuffer(
            System.IntPtr amsiContext,
                System.IntPtr buffer,
                uint length,
                [In][MarshalAs(UnmanagedType.LPWStr)] string contentName,
                System.IntPtr amsiSession,
                ref AMSI_RESULT result);
            /// amsiContext: HAMSICONTEXT->HAMSICONTEXT__*
            /// buffer: PVOID->void*
            /// length: ULONG->unsigned int
            /// contentName: LPCWSTR->WCHAR*
            /// result: AMSI_RESULT*
            [DllImport("amsi.dll", EntryPoint = "AmsiNotifyOperation", CallingConvention = CallingConvention.StdCall)]
            internal static extern int AmsiNotifyOperation(
            ///string: LPCWSTR->WCHAR*
            [DllImport("amsi.dll", EntryPoint = "AmsiScanString", CallingConvention = CallingConvention.StdCall)]
            internal static extern int AmsiScanString(
                System.IntPtr amsiContext, [In][MarshalAs(UnmanagedType.LPWStr)] string @string,
                [In][MarshalAs(UnmanagedType.LPWStr)] string contentName, System.IntPtr amsiSession, ref AMSI_RESULT result);

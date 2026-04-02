    internal static class Win32Errors
        internal const DWORD NO_ERROR = 0;
        internal const DWORD E_FAIL = 0x80004005;
        internal const DWORD TRUST_E_NOSIGNATURE = 0x800b0100;
        internal const DWORD TRUST_E_BAD_DIGEST = 0x80096010;
        internal const DWORD TRUST_E_PROVIDER_UNKNOWN = 0x800b0001;
        internal const DWORD TRUST_E_SUBJECT_FORM_UNKNOWN = 0x800B0003;
        internal const DWORD CERT_E_UNTRUSTEDROOT = 0x800b0109;
        internal const DWORD TRUST_E_EXPLICIT_DISTRUST = 0x800B0111;
        internal const DWORD CRYPT_E_BAD_MSG = 0x8009200d;
        internal const DWORD NTE_BAD_ALGID = 0x80090008;
    /// Defines the valid status flags that a signature
    /// on a file may have.
    public enum SignatureStatus
        /// The file has a valid signature.  This means only that
        /// the signature is syntactically valid.  It does not
        /// imply trust in any way.
        /// The file has an invalid signature.
        UnknownError,
        /// The file has no signature.
        NotSigned,
        /// The hash of the file does not match the hash stored
        /// along with the signature.
        HashMismatch,
        /// The certificate was signed by a publisher not trusted
        /// on the system.
        NotTrusted,
        /// The specified file format is not supported by the system
        /// for signing operations.  This usually means that the
        /// system does not know how to sign or verify the file
        /// type requested.
        NotSupportedFileFormat,
        /// The signature cannot be verified because it is incompatible
        /// with the current system.
        Incompatible
    /// Defines the valid types of signatures.
    public enum SignatureType
        /// The file is not signed.
        /// The signature is an Authenticode signature embedded into the file itself.
        Authenticode = 1,
        /// The signature is a catalog signature.
        Catalog = 2
    /// Represents a digital signature on a signed
    public sealed class Signature
        private SignatureStatus _status = SignatureStatus.UnknownError;
        private DWORD _win32Error;
        private X509Certificate2 _signerCert;
        private string _statusMessage = string.Empty;
        private X509Certificate2 _timeStamperCert;
        // private DateTime signedOn = new DateTime(0);
        // Three states:
        //   - True: we can rely on the catalog API to check catalog signature.
        //   - False: we cannot rely on the catalog API, either because it doesn't exist in the OS (win7, nano),
        //            or it's not working properly (OneCore SKUs or dev environment where powershell might
        //            be updated/refreshed).
        //   - Null: it's not determined yet whether catalog API can be relied on or not.
        internal static bool? CatalogApiAvailable = null;
        /// Gets the X509 certificate of the publisher that
        /// signed the file.
        public X509Certificate2 SignerCertificate
                return _signerCert;
        /// Gets the X509 certificate of the authority that
        /// time-stamped the file.
        public X509Certificate2 TimeStamperCertificate
                return _timeStamperCert;
        /// Gets the status of the signature on the file.
        public SignatureStatus Status
                return _status;
        /// Gets the message corresponding to the status of the
        /// signature on the file.
        public string StatusMessage
        /// Gets the path of the file to which this signature
        /// applies.
        /// Returns the signature type of the signature.
        public SignatureType SignatureType { get; internal set; }
        /// True if the item is signed as part of an operating system release.
        public bool IsOSBinary { get; internal set; }
        /// Gets the Subject Alternative Name from the signer certificate.
        public string[] SubjectAlternativeName { get; private set; }
        /// Constructor for class Signature
        /// Call this to create a validated time-stamped signature object.
        /// <param name="filePath">This signature is found in this file.</param>
        /// <param name="error">Win32 error code.</param>
        /// <param name="signer">Cert of the signer.</param>
        /// <param name="timestamper">Cert of the time stamper.</param>
        internal Signature(string filePath,
                           DWORD error,
                           X509Certificate2 signer,
                           X509Certificate2 timestamper)
            Utils.CheckArgForNullOrEmpty(filePath, "filePath");
            Utils.CheckArgForNull(signer, "signer");
            Utils.CheckArgForNull(timestamper, "timestamper");
            Init(filePath, signer, error, timestamper);
        /// Call this to create a validated signature object.
                           X509Certificate2 signer)
            Init(filePath, signer, 0, null);
        /// Call this ctor when creating an invalid signature object.
            Init(filePath, signer, error, null);
        internal Signature(string filePath, DWORD error)
            Init(filePath, null, error, null);
        private void Init(string filePath,
            _path = filePath;
            _win32Error = error;
            _signerCert = signer;
            _timeStamperCert = timestamper;
            SignatureType = SignatureType.None;
            SignatureStatus isc =
                GetSignatureStatusFromWin32Error(error);
            _status = isc;
            _statusMessage = GetSignatureStatusMessage(isc,
            // Extract Subject Alternative Name from the signer certificate
            SubjectAlternativeName = GetSubjectAlternativeName(signer);
        private static SignatureStatus GetSignatureStatusFromWin32Error(DWORD error)
            SignatureStatus isc = SignatureStatus.UnknownError;
                case Win32Errors.NO_ERROR:
                    isc = SignatureStatus.Valid;
                case Win32Errors.NTE_BAD_ALGID:
                    isc = SignatureStatus.Incompatible;
                case Win32Errors.TRUST_E_NOSIGNATURE:
                    isc = SignatureStatus.NotSigned;
                case Win32Errors.TRUST_E_BAD_DIGEST:
                case Win32Errors.CRYPT_E_BAD_MSG:
                    isc = SignatureStatus.HashMismatch;
                case Win32Errors.TRUST_E_PROVIDER_UNKNOWN:
                    isc = SignatureStatus.NotSupportedFileFormat;
                case Win32Errors.TRUST_E_EXPLICIT_DISTRUST:
                    isc = SignatureStatus.NotTrusted;
            return isc;
        private static string GetSignatureStatusMessage(SignatureStatus status,
                                                 string filePath)
            string resourceString = null;
            string arg = null;
            switch (status)
                case SignatureStatus.Valid:
                    resourceString = MshSignature.MshSignature_Valid;
                case SignatureStatus.UnknownError:
                    int intError = SecuritySupport.GetIntFromDWORD(error);
                    Win32Exception e = new Win32Exception(intError);
                    message = e.Message;
                case SignatureStatus.Incompatible:
                        resourceString = MshSignature.MshSignature_Incompatible_HashAlgorithm;
                        resourceString = MshSignature.MshSignature_Incompatible;
                    arg = filePath;
                case SignatureStatus.NotSigned:
                    resourceString = MshSignature.MshSignature_NotSigned;
                case SignatureStatus.HashMismatch:
                    resourceString = MshSignature.MshSignature_HashMismatch;
                case SignatureStatus.NotTrusted:
                    resourceString = MshSignature.MshSignature_NotTrusted;
                case SignatureStatus.NotSupportedFileFormat:
                    resourceString = MshSignature.MshSignature_NotSupportedFileFormat;
                    arg = System.IO.Path.GetExtension(filePath);
                    if (string.IsNullOrEmpty(arg))
                        resourceString = MshSignature.MshSignature_NotSupportedFileFormat_NoExtension;
                        arg = null;
                    message = StringUtil.Format(resourceString, arg);
        /// Extracts the Subject Alternative Name from the certificate.
        /// <param name="certificate">The certificate to extract SAN from.</param>
        /// <returns>Array of SAN entries or null if not found.</returns>
        private static string[] GetSubjectAlternativeName(X509Certificate2 certificate)
            foreach (X509Extension extension in certificate.Extensions)
                if (extension.Oid != null && extension.Oid.Value == CertificateFilterInfo.SubjectAlternativeNameOid)
                    string formatted = extension.Format(multiLine: true);
                    if (string.IsNullOrEmpty(formatted))
                    return formatted.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

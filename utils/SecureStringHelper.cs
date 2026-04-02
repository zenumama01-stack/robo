    /// Helper class for secure string related functionality.
    internal static class SecureStringHelper
        // Some random hex characters to identify the beginning of a
        // V2-exported SecureString.
        internal static readonly string SecureStringExportHeader = "76492d1116743f0423413b16050a5345";
        /// Create a new SecureString based on the specified binary data.
        /// The binary data must be byte[] version of unicode char[],
        /// otherwise the results are unpredictable.
        /// <param name="data">Input data.</param>
        /// <returns>A SecureString .</returns>
        internal static SecureString New(byte[] data)
            if ((data.Length % 2) != 0)
                // If the data is not an even length, they supplied an invalid key
                string error = Serialization.InvalidKey;
                throw new PSArgumentException(error);
            char ch;
            SecureString ss = new SecureString();
            // each unicode char is 2 bytes.
            int len = data.Length / 2;
            for (int i = 0; i < len; i++)
                ch = (char)(data[2 * i + 1] * 256 + data[2 * i]);
                ss.AppendChar(ch);
                // zero out the data slots as soon as we use them
                data[2 * i] = 0;
                data[2 * i + 1] = 0;
        /// Get the contents of a SecureString as byte[]
        /// <returns>Contents of s (char[]) converted to byte[].</returns>
        internal static byte[] GetData(SecureString s)
            byte[] data = new byte[s.Length * 2];
                IntPtr ptr = Marshal.SecureStringToCoTaskMemUnicode(s);
                    Marshal.Copy(ptr, data, 0, data.Length);
        /// Encode the specified byte[] as a unicode string.
        /// Currently we use simple hex encoding but this
        /// method can be changed to use a better encoding
        /// such as base64.
        /// <param name="data">Binary data to encode.</param>
        /// <returns>A string representing encoded data.</returns>
        internal static string ByteArrayToString(byte[] data)
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        /// Convert a string obtained using ByteArrayToString()
        /// back to byte[] format.
        /// <param name="s">Encoded input string.</param>
        /// <returns>Bin data as byte[].</returns>
        internal static byte[] ByteArrayFromString(string s)
            // two hex chars per byte
            int dataLen = s.Length / 2;
            byte[] data = new byte[dataLen];
                for (int i = 0; i < dataLen; i++)
                    data[i] = byte.Parse(s.AsSpan(2 * i, 2),
                                         NumberStyles.AllowHexSpecifier,
        /// Return contents of the SecureString after encrypting
        /// using DPAPI and encoding the encrypted blob as a string.
        /// <param name="input">SecureString to protect.</param>
        /// <returns>A string (see summary) .</returns>
        internal static string Protect(SecureString input)
            Utils.CheckSecureStringArg(input, "input");
            string output = string.Empty;
            byte[] protectedData = null;
            data = GetData(input);
            // DPAPI doesn't exist on UNIX so we simply use the string as a byte-array
            protectedData = data;
            protectedData = ProtectedData.Protect(data, null,
                                                  DataProtectionScope.CurrentUser);
                data[i] = 0;
            output = ByteArrayToString(protectedData);
        /// Decrypts the specified string using DPAPI and return
        /// equivalent SecureString.
        /// The string must be obtained earlier by a call to Protect()
        /// <param name="input">Encrypted string.</param>
        /// <returns>SecureString .</returns>
        internal static SecureString Unprotect(string input)
            Utils.CheckArgForNullOrEmpty(input, "input");
            if ((input.Length % 2) != 0)
                throw PSTraceSource.NewArgumentException(nameof(input), Serialization.InvalidEncryptedString, input);
            SecureString s;
            protectedData = ByteArrayFromString(input);
            // DPAPI isn't supported in UNIX, so we just translate the byte-array back to a string
            data = protectedData;
            data = ProtectedData.Unprotect(protectedData, null,
            s = New(data);
        /// using the specified key and encoding the encrypted blob as a string.
        /// <param name="input">Input string to encrypt.</param>
        /// <param name="key">Encryption key.</param>
        /// <returns>A string (see summary).</returns>
        internal static EncryptionResult Encrypt(SecureString input, SecureString key)
            // get clear text key from the SecureString key
            byte[] keyBlob = GetData(key);
            // encrypt the data
                return Encrypt(input, keyBlob);
                Array.Clear(keyBlob);
        internal static EncryptionResult Encrypt(SecureString input, byte[] key)
            return Encrypt(input, key, null);
        internal static EncryptionResult Encrypt(SecureString input, byte[] key, byte[] iv)
            Utils.CheckKeyArg(key, "key");
            // prepare the crypto stuff. Initialization Vector is
            // randomized by default.
            using (Aes aes = Aes.Create())
                iv ??= aes.IV;
                // get clear text data from the input SecureString
                byte[] data = GetData(input);
                    using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
                    using (var sourceStream = new MemoryStream(data))
                    using (var encryptedStream = new MemoryStream())
                        // encrypt it
                        using (var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write))
                            sourceStream.CopyTo(cryptoStream);
                        // return encrypted data
                        byte[] encryptedData = encryptedStream.ToArray();
                        return new EncryptionResult(ByteArrayToString(encryptedData), Convert.ToBase64String(iv));
                    Array.Clear(data, 0, data.Length);
        /// Decrypts the specified string using the specified key
        /// and return equivalent SecureString.
        /// The string must be obtained earlier by a call to Encrypt()
        /// <param name="IV">Encryption initialization vector. If this is set to null, the method uses internally computed strong random number as IV.</param>
        internal static SecureString Decrypt(string input, SecureString key, byte[] IV)
            // decrypt the data
                return Decrypt(input, keyBlob, IV);
        internal static SecureString Decrypt(string input, byte[] key, byte[] IV)
            // prepare the crypto stuff
            using (var aes = Aes.Create())
                using (ICryptoTransform decryptor = aes.CreateDecryptor(key, IV ?? aes.IV))
                using (var encryptedStream = new MemoryStream(ByteArrayFromString(input)))
                using (var targetStream = new MemoryStream())
                    // decrypt the data and return as SecureString
                    using (var sourceStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read))
                        sourceStream.CopyTo(targetStream);
                    byte[] decryptedData = targetStream.ToArray();
                        return New(decryptedData);
                        Array.Clear(decryptedData);
        /// <summary>Creates a new <see cref="SecureString"/> from a <see cref="string"/>.</summary>
        /// <param name="plainTextString">Plain text string. Must not be null.</param>
        /// <returns>A new SecureString.</returns>
        internal static unsafe SecureString FromPlainTextString(string plainTextString)
            Debug.Assert(plainTextString is not null);
            if (plainTextString.Length == 0)
                return new SecureString();
            fixed (char* charsPtr = plainTextString)
                return new SecureString(charsPtr, plainTextString.Length);
    /// Helper class to return encryption results, and the IV used to
    /// do the encryption.
    internal class EncryptionResult
        internal EncryptionResult(string encrypted, string IV)
            EncryptedData = encrypted;
            this.IV = IV;
        /// Gets the encrypted data.
        internal string EncryptedData { get; }
        /// Gets the IV used to encrypt the data.
        internal string IV { get; }
    // The DPAPIs implemented in this section are temporary workaround.
    // CoreCLR team will bring 'ProtectedData' type to Project K eventually.
    #region DPAPI
    internal enum DataProtectionScope
        CurrentUser = 0x00,
        LocalMachine = 0x01
    internal static class ProtectedData
        /// Protect.
        public static byte[] Protect(byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
            ArgumentNullException.ThrowIfNull(userData);
            GCHandle pbDataIn = new GCHandle();
            GCHandle pOptionalEntropy = new GCHandle();
            CAPI.CRYPTOAPI_BLOB blob = new CAPI.CRYPTOAPI_BLOB();
                pbDataIn = GCHandle.Alloc(userData, GCHandleType.Pinned);
                CAPI.CRYPTOAPI_BLOB dataIn = new CAPI.CRYPTOAPI_BLOB();
                dataIn.cbData = (uint)userData.Length;
                dataIn.pbData = pbDataIn.AddrOfPinnedObject();
                CAPI.CRYPTOAPI_BLOB entropy = new CAPI.CRYPTOAPI_BLOB();
                if (optionalEntropy != null)
                    pOptionalEntropy = GCHandle.Alloc(optionalEntropy, GCHandleType.Pinned);
                    entropy.cbData = (uint)optionalEntropy.Length;
                    entropy.pbData = pOptionalEntropy.AddrOfPinnedObject();
                uint dwFlags = CAPI.CRYPTPROTECT_UI_FORBIDDEN;
                if (scope == DataProtectionScope.LocalMachine)
                    dwFlags |= CAPI.CRYPTPROTECT_LOCAL_MACHINE;
                    if (!CAPI.CryptProtectData(
                        pDataIn: new IntPtr(&dataIn),
                        szDataDescr: string.Empty,
                        pOptionalEntropy: new IntPtr(&entropy),
                        pvReserved: IntPtr.Zero,
                        pPromptStruct: IntPtr.Zero,
                        dwFlags: dwFlags,
                        pDataBlob: new IntPtr(&blob)))
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        // One of the most common reasons that DPAPI operations fail is that the user
                        // profile is not loaded (for instance in the case of impersonation or running in a
                        // service.  In those cases, throw an exception that provides more specific details
                        // about what happened.
                        if (CAPI.ErrorMayBeCausedByUnloadedProfile(lastWin32Error))
                            throw new CryptographicException("Cryptography_DpApi_ProfileMayNotBeLoaded");
                            throw new CryptographicException(lastWin32Error);
                // In some cases, the API would fail due to OOM but simply return a null pointer.
                if (blob.pbData == IntPtr.Zero)
                    throw new OutOfMemoryException();
                byte[] encryptedData = new byte[(int)blob.cbData];
                Marshal.Copy(blob.pbData, encryptedData, 0, encryptedData.Length);
                return encryptedData;
                if (pbDataIn.IsAllocated)
                    pbDataIn.Free();
                if (pOptionalEntropy.IsAllocated)
                    pOptionalEntropy.Free();
                if (blob.pbData != IntPtr.Zero)
                    CAPI.ZeroMemory(blob.pbData, blob.cbData);
                    CAPI.LocalFree(blob.pbData);
        /// Unprotect.
        public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
            ArgumentNullException.ThrowIfNull(encryptedData);
            CAPI.CRYPTOAPI_BLOB userData = new CAPI.CRYPTOAPI_BLOB();
                pbDataIn = GCHandle.Alloc(encryptedData, GCHandleType.Pinned);
                dataIn.cbData = (uint)encryptedData.Length;
                    if (!CAPI.CryptUnprotectData(
                        ppszDataDescr: IntPtr.Zero,
                        pDataBlob: new IntPtr(&userData)))
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                if (userData.pbData == IntPtr.Zero)
                byte[] data = new byte[(int)userData.cbData];
                Marshal.Copy(userData.pbData, data, 0, data.Length);
                if (userData.pbData != IntPtr.Zero)
                    CAPI.ZeroMemory(userData.pbData, userData.cbData);
                    CAPI.LocalFree(userData.pbData);
    internal static class CAPI
        internal const uint CRYPTPROTECT_UI_FORBIDDEN = 0x1;
        internal const uint CRYPTPROTECT_LOCAL_MACHINE = 0x4;
        internal const int E_FILENOTFOUND = unchecked((int)0x80070002); // File not found
        internal const int ERROR_FILE_NOT_FOUND = 2;                    // File not found
        internal struct CRYPTOAPI_BLOB
            internal uint cbData;
            internal IntPtr pbData;
        internal static bool ErrorMayBeCausedByUnloadedProfile(int errorCode)
            // CAPI returns a file not found error if the user profile is not yet loaded
            return errorCode == E_FILENOTFOUND ||
                   errorCode == ERROR_FILE_NOT_FOUND;
        [DllImport("CRYPT32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CryptProtectData(
                [In] IntPtr pDataIn,
                [In] string szDataDescr,
                [In] IntPtr pOptionalEntropy,
                [In] IntPtr pvReserved,
                [In] IntPtr pPromptStruct,
                [In] uint dwFlags,
                [In, Out] IntPtr pDataBlob);
        internal static extern bool CryptUnprotectData(
                [In] IntPtr ppszDataDescr,
        [DllImport("ntdll.dll", EntryPoint = "RtlZeroMemory", SetLastError = true)]
        internal static extern void ZeroMemory(IntPtr handle, uint length);
        [DllImport(PinvokeDllNames.LocalFreeDllName, SetLastError = true)]
        internal static extern IntPtr LocalFree(IntPtr handle);
    #endregion DPAPI

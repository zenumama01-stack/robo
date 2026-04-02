    /// Defines the valid types of PSCredentials.  Used by PromptForCredential calls.
    public enum PSCredentialTypes
        /// Generic credentials.
        Generic = 1,
        /// Credentials valid for a domain.
        Domain = 2,
        /// Default credentials.
        Default = Generic | Domain
    /// Defines the options available when prompting for credentials.  Used
    /// by PromptForCredential calls.
    public enum PSCredentialUIOptions
        /// Validates the username, but not its existence
        /// or correctness.
        Default = ValidateUserNameSyntax,
        /// Performs no validation.
        /// Validates the username, but not its existence.
        ValidateUserNameSyntax,
        /// Always prompt, even if a persisted credential was available.
        AlwaysPrompt,
        /// Username is read-only, and the user may not modify it.
        ReadOnlyUserName
    /// Declare a delegate which returns the encryption key and initialization vector for symmetric encryption algorithm.
    /// <param name="context">The streaming context, which contains the serialization context.</param>
    /// <param name="key">Symmetric encryption key.</param>
    /// <param name="iv">Symmetric encryption initialization vector.</param>
    public delegate bool GetSymmetricEncryptionKey(StreamingContext context, out byte[] key, out byte[] iv);
    /// Offers a centralized way to manage usernames, passwords, and
    /// credentials.
    [Serializable]
    public sealed class PSCredential : ISerializable
        /// Gets or sets a delegate which returns the encryption key and initialization vector for symmetric encryption algorithm.
        public static GetSymmetricEncryptionKey GetSymmetricEncryptionKeyDelegate
                return s_delegate;
                s_delegate = value;
        private static GetSymmetricEncryptionKey s_delegate = null;
        /// GetObjectData.
        public void GetObjectData(SerializationInfo info, StreamingContext context)
            // serialize the secure string
            string safePassword = string.Empty;
            if (_password != null && _password.Length > 0)
                byte[] key;
                byte[] iv;
                if (s_delegate != null && s_delegate(context, out key, out iv))
                    safePassword = SecureStringHelper.Encrypt(_password, key, iv).EncryptedData;
                        safePassword = SecureStringHelper.Protect(_password);
                    catch (CryptographicException cryptographicException)
                        throw PSTraceSource.NewInvalidOperationException(cryptographicException, Credential.CredentialDisallowed);
            info.AddValue("UserName", _userName);
            info.AddValue("Password", safePassword);
        /// PSCredential.
        private PSCredential(SerializationInfo info, StreamingContext context)
            _userName = (string)info.GetValue("UserName", typeof(string));
            // deserialize to secure string
            string safePassword = (string)info.GetValue("Password", typeof(string));
            if (safePassword == string.Empty)
                _password = new SecureString();
                    _password = SecureStringHelper.Decrypt(safePassword, key, iv);
                    _password = SecureStringHelper.Unprotect(safePassword);
        private readonly string _userName;
        private readonly SecureString _password;
        /// User's name.
        /// User's password.
        public SecureString Password
            get { return _password; }
        /// Initializes a new instance of the PSCredential class with a
        /// username and password.
        /// <param name="userName">User's name.</param>
        /// <param name="password">User's password.</param>
        public PSCredential(string userName, SecureString password)
            Utils.CheckArgForNullOrEmpty(userName, "userName");
            Utils.CheckArgForNull(password, "password");
            _userName = userName;
            _password = password;
        /// username and password from PSObject.
        /// <param name="pso"></param>
        public PSCredential(PSObject pso)
                throw PSTraceSource.NewArgumentNullException(nameof(pso));
            if (pso.Properties["UserName"] != null)
                _userName = (string)pso.Properties["UserName"].Value;
                if (pso.Properties["Password"] != null)
                    _password = (SecureString)pso.Properties["Password"].Value;
        /// Initializes a new instance of the PSCredential class.
        private PSCredential()
        private NetworkCredential _netCred;
        /// Returns an equivalent NetworkCredential object for this
        /// A null is returned if
        /// -- current object has not been initialized
        /// -- current creds are not compatible with NetworkCredential
        ///    (such as smart card creds or cert creds)
        ///     null if the current object has not been initialized.
        ///     null if the current credentials are incompatible with
        ///       a NetworkCredential -- such as smart card credentials.
        ///     the appropriate network credential for this PSCredential otherwise.
        public NetworkCredential GetNetworkCredential()
            if (_netCred == null)
                string user = null;
                string domain = null;
                if (IsValidUserName(_userName, out user, out domain))
                    _netCred = new NetworkCredential(user, _password, domain);
            return _netCred;
        /// Provides an explicit cast to get a NetworkCredential
        /// from this PSCredential.
        /// <param name="credential">PSCredential to convert.</param>
        public static explicit operator NetworkCredential(PSCredential credential)
            if (credential == null)
                throw PSTraceSource.NewArgumentNullException("credential");
            return credential.GetNetworkCredential();
        /// Gets an empty PSCredential.  This is an PSCredential with both UserName
        /// and Password initialized to null.
        public static PSCredential Empty
                return s_empty;
        private static readonly PSCredential s_empty = new PSCredential();
        /// Parse a string that represents a fully qualified username
        /// to verify that it is syntactically valid. We only support
        /// two formats:
        /// -- domain\user
        /// -- user@domain
        /// for any other format, we simply treat the entire string
        /// as user name and set domain name to "".
        private static bool IsValidUserName(string input,
                                            out string user,
                                            out string domain)
            if (string.IsNullOrEmpty(input))
                user = domain = null;
            SplitUserDomain(input, out user, out domain);
            if ((user == null) ||
                (domain == null) ||
                (user.Length == 0))
                // UserName is the public property of Credential object. Use this as
                // parameter name in error
                // See bug NTRAID#Windows OS Bugs-1106386-2005/03/25-hiteshr
                throw PSTraceSource.NewArgumentException("UserName", Credential.InvalidUserNameFormat);
        /// Split a given string into its user and domain
        /// components. Supported formats are:
        /// With any other format, the entire input is treated as user
        /// name and domain is set to "".
        /// In any case, the function does not check if the split string
        /// are really valid as user or domain names.
        private static void SplitUserDomain(string input,
            domain = null;
            if ((i = input.IndexOf('\\')) >= 0)
                user = input.Substring(i + 1);
                domain = input.Substring(0, i);
            // In V1 and V2, we had a bug where email addresses (i.e. foo@bar.com)
            // were being split into Username=Foo, Domain=bar.com.
            // This was breaking apps (i.e.: Exchange), so we need to make
            // Username = foo@bar.com if the domain has a dot in it (since
            // domains can't have dots).
            // HOWEVER, there was a workaround for this bug in v1 and v2, where the
            // cred could be entered as "foo@bar.com@bar.com" - making:
            // Username = foo@bar.com, Domain = bar.com
            // We need to keep the behaviour in this case.
            i = input.LastIndexOf('@');
                (i >= 0) &&
                    (input.LastIndexOf('.') < i) ||
                    (input.IndexOf('@') != i)
                domain = input.Substring(i + 1);
                user = input.Substring(0, i);
                user = input;
                domain = string.Empty;

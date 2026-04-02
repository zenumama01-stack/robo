using System.Xml.XPath;
using Runspaces = System.Management.Automation.Runspaces;
using SMASecurity = System.Management.Automation.Security;
    /// Defines the Certificate Provider dynamic parameters.
    /// We only support one dynamic parameter for Win 7 and earlier:
    /// CodeSigningCert
    /// If provided, we only return certificates valid for signing code or
    /// scripts.
    internal sealed class CertificateProviderDynamicParameters
        /// Gets or sets a switch that controls whether we only return
        /// code signing certs.
        public SwitchParameter CodeSigningCert
            get { return _codeSigningCert; }
            set { _codeSigningCert = value; }
        private SwitchParameter _codeSigningCert = new();
        /// Gets or sets a filter that controls whether we only return
        /// data encipherment certs.
        public SwitchParameter DocumentEncryptionCert
        /// server authentication certs.
        public SwitchParameter SSLServerAuthentication
        /// Gets or sets a filter by DNSName.
        /// Expected content is a single DNS Name that may start and/or end
        /// with '*': "contoso.com" or "*toso.c*".
        /// All WildcardPattern class features supported.
        public string DnsName
        /// Gets or sets a filter by EKU.
        /// Expected content is one or more OID strings:
        /// "1.3.6.1.5.5.7.3.1", "*Server*", etc.
        /// For a cert to match, it must be valid for all listed OIDs.
        public string[] Eku
        /// Gets or sets a filter by the number of valid days.
        /// Expected content is a non-negative integer.
        /// "0" matches all certs that have already expired.
        /// "1" matches all certs that are currently valid and will expire
        /// by next day (local time).
        public int ExpiringInDays
        } = -1;
    /// Defines the type of DNS string
    /// The structure contains punycode name and unicode name.
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public readonly struct DnsNameRepresentation
        /// Punycode version of DNS name.
        private readonly string _punycodeName;
        /// Unicode version of DNS name.
        private readonly string _unicodeName;
        /// Ambiguous constructor of a DnsNameRepresentation.
        public DnsNameRepresentation(string inputDnsName)
            _punycodeName = inputDnsName;
            _unicodeName = inputDnsName;
        /// Specific constructor of a DnsNameRepresentation.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Punycode")]
        public DnsNameRepresentation(
            string inputPunycodeName,
            string inputUnicodeName)
            _punycodeName = inputPunycodeName;
            _unicodeName = inputUnicodeName;
        /// Value comparison.
        public bool Equals(DnsNameRepresentation dnsName)
            if (_unicodeName != null && dnsName._unicodeName != null)
                            _unicodeName,
                            dnsName._unicodeName,
                            StringComparison.OrdinalIgnoreCase))
            else if (_unicodeName == null && dnsName._unicodeName == null)
        /// Get property of Punycode.
        public string Punycode
                return _punycodeName;
        /// Get property of Unicode.
        public string Unicode
                return _unicodeName;
        /// Get display string.
            // Use case sensitive comparison here.
            // We don't ever expect to see the punycode and unicode strings
            // to differ only by upper/lower case.  If they do, that's really
            // a code bug, and the effect is to just display both strings.
            return string.Equals(_punycodeName, _unicodeName, StringComparison.Ordinal)
                ? _punycodeName
                : _unicodeName + " (" + _punycodeName + ")";
    /// Defines the Certificate Provider remove-item dynamic parameters.
    /// Currently, we only support one dynamic parameter: DeleteKey
    /// If provided, we will delete the private key when we remove a certificate.
    internal sealed class ProviderRemoveItemDynamicParameters
        /// Switch that controls whether we should delete private key
        /// when remove a certificate.
        public SwitchParameter DeleteKey
                    return _deleteKey;
                    _deleteKey = value;
        private SwitchParameter _deleteKey = new();
    /// Defines the safe handle class for native cert store handles,
    /// HCERTSTORE.
    internal sealed class CertificateStoreHandle : SafeHandle
        public CertificateStoreHandle() : base(IntPtr.Zero, true)
            get { return handle == IntPtr.Zero; }
            bool fResult = false;
            if (handle != IntPtr.Zero)
                fResult = SMASecurity.NativeMethods.CertCloseStore(handle, 0);
            return fResult;
        public IntPtr Handle
            get { return handle; }
            set { handle = value; }
    /// Defines the Certificate Provider store handle class.
    internal sealed class X509NativeStore
        // #region tracer
        /// Initializes a new instance of the X509NativeStore class.
        public X509NativeStore(X509StoreLocation StoreLocation, string StoreName)
            _storeLocation = StoreLocation;
            _storeName = StoreName;
        public void Open(bool includeArchivedCerts)
            if (_storeHandle != null && _archivedCerts != includeArchivedCerts)
                _storeHandle = null;        // release the old handle
            if (_storeHandle == null)
                _valid = false;
                _open = false;
                SMASecurity.NativeMethods.CertOpenStoreFlags StoreFlags =
                    SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_SHARE_STORE_FLAG |
                    SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_SHARE_CONTEXT_FLAG |
                    SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_OPEN_EXISTING_FLAG |
                    SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_MAXIMUM_ALLOWED_FLAG;
                if (includeArchivedCerts)
                    StoreFlags |= SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_ENUM_ARCHIVED_FLAG;
                switch (_storeLocation.Location)
                    case StoreLocation.LocalMachine:
                        StoreFlags |= SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_SYSTEM_STORE_LOCAL_MACHINE;
                    case StoreLocation.CurrentUser:
                        StoreFlags |= SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_SYSTEM_STORE_CURRENT_USER;
                        // ThrowItemNotFound(storeLocation.ToString(), CertificateProviderItem.StoreLocation);
                IntPtr hCertStore = SMASecurity.NativeMethods.CertOpenStore(
                                SMASecurity.NativeMethods.CertOpenStoreProvider.CERT_STORE_PROV_SYSTEM,
                                SMASecurity.NativeMethods.CertOpenStoreEncodingType.X509_ASN_ENCODING,
                                IntPtr.Zero,  // hCryptProv
                                StoreFlags,
                                _storeName);
                if (hCertStore == IntPtr.Zero)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                _storeHandle = new CertificateStoreHandle();
                _storeHandle.Handle = hCertStore;
                // we only do CertControlStore for stores other than UserDS
                if (!string.Equals(
                                _storeName,
                                "UserDS",
                    if (!SMASecurity.NativeMethods.CertControlStore(
                                _storeHandle.Handle,
                                SMASecurity.NativeMethods.CertControlStoreType.CERT_STORE_CTRL_AUTO_RESYNC,
                                IntPtr.Zero))
                        _storeHandle = null;
                _valid = true;
                _open = true;
                _archivedCerts = includeArchivedCerts;
        public IntPtr GetFirstCert()
            return GetNextCert(IntPtr.Zero);
        public IntPtr GetNextCert(IntPtr certContext)
            if (!_open)
                throw Marshal.GetExceptionForHR(
                                    SMASecurity.NativeMethods.CRYPT_E_NOT_FOUND);
            if (Valid)
                certContext = SMASecurity.NativeMethods.CertEnumCertificatesInStore(
                                                    certContext);
                certContext = IntPtr.Zero;
            return certContext;
        public IntPtr GetCertByName(string Name)
            IntPtr certContext = IntPtr.Zero;
                if (DownLevelHelper.HashLookupSupported())
                    certContext = SMASecurity.NativeMethods.CertFindCertificateInStore(
                            0,                                // dwFindFlags
                            SMASecurity.NativeMethods.CertFindType.CERT_FIND_HASH_STR,
                            IntPtr.Zero);                     // pPrevCertContext
                    // the pre-Win8 CAPI2 code does not provide an easy way
                    // to directly access a specific certificate.
                    // We have to iterate through all certs to find
                    // what we want.
                        certContext = GetNextCert(certContext);
                        if (certContext == IntPtr.Zero)
                        X509Certificate2 cert = new(certContext);
                                    cert.Thumbprint,
        public void FreeCert(IntPtr certContext)
            SMASecurity.NativeMethods.CertFreeCertificateContext(certContext);
        /// Native IntPtr store handle.
        public IntPtr StoreHandle
                return _storeHandle.Handle;
        /// X509StoreLocation store location.
        public X509StoreLocation Location
                return _storeLocation;
        /// String store name.
        public string StoreName
                return _storeName;
        /// True if a real store is open.
        public bool Valid
                return _valid;
        private bool _archivedCerts = false;
        private readonly X509StoreLocation _storeLocation = null;
        private readonly string _storeName = null;
        private CertificateStoreHandle _storeHandle = null;
        private bool _valid = false;
        private bool _open = false;
    /// Defines the types of items
    /// supported by the certificate provider.
    internal enum CertificateProviderItem
        /// An unknown item.
        /// An X509 Certificate.
        Certificate,
        /// A certificate store location.
        /// For example, cert:\CurrentUser.
        Store,
        /// A certificate store.
        /// For example, cert:\CurrentUser\My.
        StoreLocation
    /// Defines the implementation of a Certificate Store Provider.  This provider
    /// allows for stateless namespace navigation of the computer's certificate
    /// store.
    [CmdletProvider("Certificate", ProviderCapabilities.ShouldProcess)]
    [OutputType(typeof(string), typeof(PathInfo), ProviderCmdlet = ProviderCmdlet.ResolvePath)]
    [OutputType(typeof(PathInfo), ProviderCmdlet = ProviderCmdlet.PushLocation)]
    [OutputType(typeof(PathInfo), ProviderCmdlet = ProviderCmdlet.PopLocation)]
    [OutputType(typeof(Microsoft.PowerShell.Commands.X509StoreLocation), typeof(X509Certificate2), ProviderCmdlet = ProviderCmdlet.GetItem)]
    [OutputType(typeof(X509Store), typeof(X509Certificate2), ProviderCmdlet = ProviderCmdlet.GetChildItem)]
    public sealed class CertificateProvider : NavigationCmdletProvider, ICmdletProviderSupportsHelp
        /// Tracer for certificate provider.
        [TraceSource("CertificateProvider",
                      "The core command provider for certificates")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("CertificateProvider",
                      "The core command provider for certificates");
        /// Indicate if we already have attempted to load the PKI module.
        private bool _hasAttemptedToLoadPkiModule = false;
        /// Lock that guards access to the following static members
        /// -- storeLocations
        /// -- pathCache.
        private static readonly object s_staticLock = new();
        /// List of store locations. They do not change once initialized.
        /// Synchronized on staticLock.
        private static List<X509StoreLocation> s_storeLocations = null;
        /// Cache that stores paths and their associated objects.
        /// key is full path to store-location/store/certificate
        /// value is X509StoreLocation/X509NativeStore/X509Certificate2 object
        private static Hashtable s_pathCache = null;
        /// We allow either / or \ to be the path separator.
        private static readonly char[] s_pathSeparators = new char[] { '/', '\\' };
        /// Regex pattern that defines a valid cert path.
        private const string certPathPattern = @"^\\((?<StoreLocation>CurrentUser|LocalMachine)(\\(?<StoreName>[a-zA-Z]+)(\\(?<Thumbprint>[0-9a-f]{40}))?)?)?$";
        /// Cache the store handle to avoid repeated CertOpenStore calls.
        private static X509NativeStore s_storeCache = null;
        /// On demand create the Regex to avoid a hit to startup perf.
        /// Note, its OK that staticLock is being used here because only
        /// IsValidPath is calling this static property so we shouldn't
        /// have any deadlocks due to other locked static members calling
        /// this property.
        private static Regex s_certPathRegex = null;
        private static Regex CertPathRegex
                lock (s_staticLock)
                    if (s_certPathRegex == null)
                        const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
                        s_certPathRegex = new Regex(certPathPattern, options);
                return s_certPathRegex;
        /// Initializes a new instance of the CertificateProvider class.
        /// This initializes the default certificate store locations.
        public CertificateProvider()
            // initialize storeLocations list and also update the cache
                if (s_storeLocations == null)
                    s_pathCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    s_storeLocations =
                        new List<X509StoreLocation>();
                    // create and cache CurrentUser store-location
                    X509StoreLocation user = new(StoreLocation.CurrentUser);
                    s_storeLocations.Add(user);
                    AddItemToCache(nameof(StoreLocation.CurrentUser),
                    // create and cache LocalMachine store-location
                    X509StoreLocation machine = new(StoreLocation.LocalMachine);
                    s_storeLocations.Add(machine);
                    AddItemToCache(nameof(StoreLocation.LocalMachine),
                                   machine);
                    AddItemToCache(string.Empty, s_storeLocations);
        /// Removes an item at the specified path.
        /// The path of the item to remove.
        /// <param name="recurse">
        /// Recursively remove.
        /// Nothing.
        /// <exception cref="System.ArgumentException">
        ///     path is null or empty.
        ///     destination is null or empty.
        protected override void RemoveItem(
                                bool recurse)
            path = NormalizePath(path);
            bool isContainer = false;
            bool fDeleteKey = false;
            object outObj = GetItemAtPath(path, false, out isContainer);
            string[] pathElements = GetPathElements(path);
            bool fUserContext = string.Equals(pathElements[0], "CurrentUser", StringComparison.OrdinalIgnoreCase);
            // isContainer = true means not a valid certificate
            // if source store is user root store and UI is not allowed
            // we raise invalid operation
            if (DetectUIHelper.GetOwnerWindow(Host) == IntPtr.Zero && fUserContext &&
                 string.Equals(pathElements[1], "ROOT", StringComparison.OrdinalIgnoreCase))
                string message = CertificateProviderStrings.UINotAllowed;
                const string errorId = "UINotAllowed";
                ThrowInvalidOperation(errorId, message);
            if (DynamicParameters != null && DynamicParameters is ProviderRemoveItemDynamicParameters dp)
                if (dp.DeleteKey)
                    fDeleteKey = true;
            if (isContainer)
                if (pathElements.Length == 2) // is a store
                    // not support user context
                    if (fUserContext)
                        string message = CertificateProviderStrings.CannotDeleteUserStore;
                        const string errorId = "CannotDeleteUserStore";
                    RemoveCertStore(pathElements[1], fDeleteKey, path);
                else // other container than a store
                    string message = CertificateProviderStrings.CannotRemoveContainer;
                    const string errorId = "CannotRemoveContainer";
            else // certificate
                // do remove
                X509Certificate2 certificate = outObj as X509Certificate2;
                RemoveCertItem(certificate, fDeleteKey, !fUserContext, path);
        /// Gets the dynamic parameters for remove-item on the Certificate
        /// Provider.  We currently only support one dynamic parameter,
        /// "DeleteKey," that delete private key when we delete a certificate.
        /// If the path was specified on the command line, this is the path
        /// to the item for which to get the dynamic parameters.
        /// Ignored.
        /// An object that has properties and fields decorated with
        /// parsing attributes similar to a cmdlet class.
        protected override object RemoveItemDynamicParameters(string path, bool recurse)
            return new ProviderRemoveItemDynamicParameters();
        /// Moves an item at the specified path to the given destination.
        /// The path of the item to move.
        /// The path of the destination.
        /// Nothing.  Moved items are written to the context's pipeline.
        protected override void MoveItem(
                                string destination)
            // normalize path
            destination = NormalizePath(destination);
            // get elements from the path
            string[] destElements = GetPathElements(destination);
            object cert = GetItemAtPath(path, false, out isContainer);
            // isContainer = true; means an invalid path
                string message = CertificateProviderStrings.CannotMoveContainer;
                const string errorId = "CannotMoveContainer";
            if (destElements.Length != 2) // not a store
                // if the destination leads to the same thumbprint
                if (destElements.Length == 3 &&
                   (string.Equals(pathElements[2], destElements[2], StringComparison.OrdinalIgnoreCase)))
                    // in this case we think of destination path as valid
                    // and strip the thumbprint part
                    destination = Path.GetDirectoryName(destination);
                    string message = CertificateProviderStrings.InvalidDestStore;
                    const string errorId = "InvalidDestStore";
            // the second element is store location
            // we do not allow cross context move
            // we do not allow the destination store is the same as source
            if (!string.Equals(pathElements[0], destElements[0], StringComparison.OrdinalIgnoreCase))
                string message = CertificateProviderStrings.CannotMoveCrossContext;
                const string errorId = "CannotMoveCrossContext";
            if (string.Equals(pathElements[1], destElements[1], StringComparison.OrdinalIgnoreCase))
                string message = CertificateProviderStrings.CannotMoveToSameStore;
                const string errorId = "CannotMoveToSameStore";
            // if source or destination store is user root store and UI is not allowed
            if (DetectUIHelper.GetOwnerWindow(Host) == IntPtr.Zero)
                if ((string.Equals(pathElements[0], "CurrentUser", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(pathElements[1], "ROOT", StringComparison.OrdinalIgnoreCase)) ||
                     (string.Equals(destElements[0], "CurrentUser", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(destElements[1], "ROOT", StringComparison.OrdinalIgnoreCase)))
            if (cert != null) // we get cert
                // get destination store
                bool isDestContainer = false;
                object store = GetItemAtPath(destination, false, out isDestContainer);
                X509Certificate2 certificate = cert as X509Certificate2;
                if (store is X509NativeStore certstore)
                    certstore.Open(true);
                    string action = CertificateProviderStrings.Action_Move;
                    string resource = string.Format(
                                          CertificateProviderStrings.MoveItemTemplate,
                                          destination);
                        DoMove(destination, certificate, certstore, path);
                ThrowItemNotFound(path, CertificateProviderItem.Certificate);
        /// Creates a certificate store with the given path.
        /// New-Item doesn't go through the method "ItemExists". But for the
        /// CertificateProvider, New-Item can create an X509Store and return
        /// it, and the user can access the certificates within the store via its
        /// property "Certificates". We want the extra new properties of the
        /// X509Certificate2 objects to be shown to the user, so we also need
        /// to import the PKI module in this method, if we haven't tried it yet.
        /// The path of the certificate store to create.
        /// Only support store.
        /// Ignored
        /// Nothing.  The new certificate store object is
        /// written to the context's pipeline.
        protected override void NewItem(
                string type,
                object value)
            if (!_hasAttemptedToLoadPkiModule)
                // Attempt to load the PKI module if we haven't tried yet
                AttemptToImportPkiModule();
            // get the elements from the path
            // only support creating store
            if (pathElements.Length != 2)
                string message = CertificateProviderStrings.CannotCreateItem;
                const string errorId = "CannotCreateItem";
                string message = CertificateProviderStrings.CannotCreateUserStore;
                const string errorId = "CannotCreateUserStore";
            const SMASecurity.NativeMethods.CertOpenStoreFlags StoreFlags =
                    SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_CREATE_NEW_FLAG |
                    SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_MAXIMUM_ALLOWED_FLAG |
                    SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_SYSTEM_STORE_LOCAL_MACHINE;
            // Create new store
                                pathElements[1]);
            else // free native store handle
                fResult = SMASecurity.NativeMethods.CertCloseStore(hCertStore, 0);
            X509Store outStore = new(pathElements[1], StoreLocation.LocalMachine);
            WriteItemObject(outStore, path, true);
        #region DriveCmdletProvider overrides
        /// Initializes the cert: drive.
        /// A collection that contains the PSDriveInfo object
        /// that represents the cert: drive.
        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
            string providerDescription = CertificateProviderStrings.CertProvidername;
            PSDriveInfo drive = new(
                name: "Cert",
                provider: ProviderInfo,
                root: @"\",
                providerDescription,
                credential: null);
            Collection<PSDriveInfo> drives = new();
            drives.Add(drive);
            return drives;
        /// Determines if the item at the given path is a store-location
        /// or store with items in it.
        /// The full path to the item.
        /// True if the path refers to a store location, or store that contains
        /// certificates.  False otherwise.
        /// <exception cref="System.ArgumentNullException">
        /// Path is null
        /// <exception cref="System.Security.Cryptography.CryptographicException">
        /// This exception can be thrown if any cryptographic error occurs.
        /// It is not possible to know exactly what went wrong.
        /// This is because of the way CryptographicException is designed.
        /// Some example reasons include:
        ///  -- certificate is invalid
        ///  -- certificate has no private key
        ///  -- certificate password mismatch
        protected override bool HasChildItems(string path)
            Utils.CheckArgForNull(path, "path");
            if (path.Length == 0)
            object item = GetItemAtPath(path, false, out isContainer);
            if ((item != null) && isContainer)
                if (item is X509StoreLocation storeLocation)
                    result = storeLocation.StoreNames.Count > 0;
                else if (item is X509NativeStore store)
                    store.Open(IncludeArchivedCerts());
                    IntPtr certContext = store.GetFirstCert();
                    if (certContext != IntPtr.Zero)
                        store.FreeCert(certContext);
        /// Determines if the specified path is syntactically and semantically valid.
        /// An example path looks like this:
        ///     cert:\CurrentUser\My\5F98EBBFE735CDDAE00E33E0FD69050EF9220254.
        /// The path of the item to check.
        /// True if the path is valid, false otherwise.
        protected override bool IsValidPath(string path)
            path = EnsureDriveIsRooted(path);
            bool isCertPath = CertPathRegex.Match(path).Success;
            return isCertPath;
        /// Determines if the store location, store, or certificate exists
        /// at the specified path.
        /// The method ItemExists will be hit by all built-in cmdlets that interact
        /// with the CertificateProvider except for the New-Item. They are:
        ///     Get-ChildItem
        ///     Set-Location
        ///     Push-Location
        ///     Pop-Location
        ///     Move-Item
        ///     Invoke-Item
        ///     Get-Item
        ///     Remove-Item
        /// So we import the PKI module in this method if we haven't tried yet.
        /// True if a the store location, store, or certificate exists
        /// at the specified path.  False otherwise.
        /// Possible reasons:
        ///  -- etc
        protected override bool ItemExists(string path)
                // We fetch the item to see if it exists. This is
                // because the managed cert infrastructure does not
                // provide a way to test for existence.
                    item = GetItemAtPath(path, true, out isContainer);
                catch (ProviderInvocationException e)
                    // if the item is not found, we get ProviderInvocationException
                    // with inner exception set to CertificateProviderItemNotFoundException
                    // If the inner exception is not of that type
                    // then we need to rethrow
                    if (e.InnerException is not CertificateProviderItemNotFoundException)
                result = (bool)item;
            s_tracer.WriteLine("result = {0}", result);
        /// Gets the store location, store, or certificate
        /// The path of the item to retrieve.
        protected override void GetItem(string path)
            CertificateFilterInfo filter = GetFilter();
                if (!isContainer) // certificate
                    // If the filter is null, output the certificate we got.
                    if (filter == null)
                        WriteItemObject(item, path, isContainer);
                        // The filter is non null. If the certificate
                        // satisfies the filter, output it. Otherwise, don't.
                        X509Certificate2 cert = item as X509Certificate2;
                        Dbg.Diagnostics.Assert(cert != null, "item should be a certificate");
                        if (MatchesFilter(cert, filter))
                else  // container
                    // The item is a container. If the filter is non null, we don't output it.
                    if (item is X509StoreLocation storeLocation)  // store location
                    else if (item is X509NativeStore store) // store
                        // create X509Store
                        X509Store outStore = new(store.StoreName, store.Location.Location);
                        WriteItemObject(outStore, path, isContainer);
        /// Gets the parent of the given path.
        /// The path of which to get the parent.
        /// <param name="root">
        /// The root of the drive.
        /// The parent of the given path.
        protected override string GetParentPath(string path, string root)
            string parentPath = base.GetParentPath(path, root);
            return parentPath;
        /// Gets the name of the leaf element of the specified path.
        /// The fully qualified path to the item.
        /// The leaf element of the specified path.
        protected override string GetChildName(string path)
            // Path for root is empty string
            if (path != null && path.Length == 0)
                return MyGetChildName(path);
        /// We want to import the PKI module explicitly because a type for X509Certificate
        /// is defined in the PKI module that add new properties to the X509Certificate2
        /// objects. We want to show those new properties to the user without requiring
        /// someone to force the loading of this module.
        private void AttemptToImportPkiModule()
            const string moduleName = "pki";
            if (Runspaces.Runspace.DefaultRunspace == null)
                // Requires default runspace. Only import the module.
                // when a default runspace is available.
            CommandInfo commandInfo =
                new CmdletInfo(
                    "Import-Module",
                     typeof(Microsoft.PowerShell.Commands.ImportModuleCommand));
            Runspaces.Command importModuleCommand = new(commandInfo);
            s_tracer.WriteLine("Attempting to load module: {0}", moduleName);
                System.Management.Automation.PowerShell ps = null;
                ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace)
                            .AddCommand(importModuleCommand)
                                .AddParameter("Name", moduleName)
                                .AddParameter("Scope", StringLiterals.Global)
                                .AddParameter("ErrorAction", ActionPreference.Ignore)
                                .AddParameter("WarningAction", ActionPreference.Ignore)
                                .AddParameter("InformationAction", ActionPreference.Ignore)
                                .AddParameter("Verbose", false)
                                .AddParameter("Debug", false);
                ps.Invoke();
            _hasAttemptedToLoadPkiModule = true;
        private static string MyGetChildName(string path)
            // Verify the parameters
                throw PSTraceSource.NewArgumentException(nameof(path));
            // Normalize the path
            path = path.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
            // Trim trailing back slashes
            path = path.TrimEnd(StringLiterals.DefaultPathSeparator);
            int separatorIndex = path.LastIndexOf(StringLiterals.DefaultPathSeparator);
            // Since there was no path separator return the entire path
            if (separatorIndex == -1)
                result = path;
                result = path.Substring(separatorIndex + 1);
        /// Invokes the certificate management UI (certmgr.msc)
        /// for any path.
        protected override void InvokeDefaultAction(string path)
            string action = CertificateProviderStrings.Action_Invoke;
            const string certmgr = "certmgr.msc";
            string certPath = System.IO.Path.Combine(
                System.Environment.ExpandEnvironmentVariables("%windir%"), "system32");
            if (ShouldProcess(path, action))
                System.Diagnostics.Process.Start(System.IO.Path.Combine(certPath, certmgr));
        private static string EnsureDriveIsRooted(string path)
            // Find the drive separator
            int index = path.IndexOf(':');
                // if the drive separator is the end of the path, add
                // the root path separator back
                if (index + 1 == path.Length)
                    result = path + StringLiterals.DefaultPathSeparator;
            else if ((path.Length == 0) || (path[0] != StringLiterals.DefaultPathSeparator))
                result = StringLiterals.DefaultPathSeparator + path;
        private static ErrorRecord CreateErrorRecord(string path,
                                              CertificateProviderItem itemType)
            string message = null;
            // first, find the resource-id so that we can display
            // correct message
            switch (itemType)
                case CertificateProviderItem.Certificate:
                    message = CertificateProviderStrings.CertificateNotFound;
                case CertificateProviderItem.Store:
                    message = CertificateProviderStrings.CertificateStoreNotFound;
                case CertificateProviderItem.StoreLocation:
                    message = CertificateProviderStrings.CertificateStoreLocationNotFound;
                    message = CertificateProviderStrings.InvalidPath;
            message = string.Format(
                message, path);
            ErrorDetails ed = new(message);
            // create appropriate exception type
                    e = new CertificateNotFoundException(message);
                    e = new CertificateStoreNotFoundException(message);
                    e = new CertificateStoreLocationNotFoundException(message);
                    e = new ArgumentException(message);
                "CertProviderItemNotFound",
            er.ErrorDetails = ed;
            return er;
        private void ThrowErrorRemoting(int stat)
            if (this.Host.Name.Equals("ServerRemoteHost", StringComparison.OrdinalIgnoreCase))
                Exception e = new System.ComponentModel.Win32Exception(stat);
                string error = e.Message;
                string message = CertificateProviderStrings.RemoteErrorMessage;
                error += message;
                Exception e2 = new(error);
                            e2,
                            "RemotingFailure",
                throw new System.ComponentModel.Win32Exception(stat);
        private void ThrowInvalidOperation(string errorId, string message)
        private void ThrowItemNotFound(string path,
            ErrorRecord er = CreateErrorRecord(path, itemType);
        private static string NormalizePath(string path)
            if (path.Length > 0)
                char lastChar = path[path.Length - 1];
                if ((lastChar == '/') || (lastChar == '\\'))
                    path = path.Substring(0, path.Length - 1);
                string[] elts = GetPathElements(path);
                path = string.Join('\\', elts);
        private static string[] GetPathElements(string path)
            string[] allElts = path.Split(s_pathSeparators);
            string[] result = null;
            Stack<string> elts = new();
            foreach (string e in allElts)
                if ((e == ".") || (e == string.Empty))
                else if (e == "..")
                    if (elts.Count > 0)
                        elts.Pop();
                    elts.Push(e);
            result = elts.ToArray();
            Array.Reverse(result);
        /// Delete private key.
        /// <param name="pProvInfo">Key prov info.</param>
        /// <returns>No return.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Management.Automation.Security.NativeMethods.NCryptSetProperty(System.IntPtr,System.String,System.Void*,System.Int32,System.Int32)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Management.Automation.Security.NativeMethods.NCryptFreeObject(System.IntPtr)")]
        private void DoDeleteKey(IntPtr pProvInfo)
            IntPtr hProv = IntPtr.Zero;
            SMASecurity.NativeMethods.CRYPT_KEY_PROV_INFO keyProvInfo =
                Marshal.PtrToStructure<SMASecurity.NativeMethods.CRYPT_KEY_PROV_INFO>(pProvInfo);
            IntPtr hWnd = DetectUIHelper.GetOwnerWindow(Host);
            if (keyProvInfo.dwProvType != 0) // legacy
                if (hWnd != IntPtr.Zero)
                    if (SMASecurity.NativeMethods.CryptAcquireContext(
                        ref hProv,
                        keyProvInfo.pwszContainerName,
                        keyProvInfo.pwszProvName,
                        (int)keyProvInfo.dwProvType,
                        (uint)SMASecurity.NativeMethods.ProviderFlagsEnum.CRYPT_VERIFYCONTEXT))
                            void* pWnd = hWnd.ToPointer();
                            SMASecurity.NativeMethods.CryptSetProvParam(
                                hProv,
                                SMASecurity.NativeMethods.ProviderParam.PP_CLIENT_HWND,
                                &pWnd,
                            SMASecurity.NativeMethods.CryptReleaseContext(hProv, 0);
                if (!SMASecurity.NativeMethods.CryptAcquireContext(
                                keyProvInfo.dwFlags | (uint)SMASecurity.NativeMethods.ProviderFlagsEnum.CRYPT_DELETEKEYSET |
                                (hWnd == IntPtr.Zero ? (uint)SMASecurity.NativeMethods.ProviderFlagsEnum.CRYPT_SILENT : 0)))
                    ThrowErrorRemoting(Marshal.GetLastWin32Error());
            else  // cng key
                uint cngKeyFlag = 0;
                IntPtr hCNGProv = IntPtr.Zero;
                IntPtr hCNGKey = IntPtr.Zero;
                if ((keyProvInfo.dwFlags & (uint)SMASecurity.NativeMethods.ProviderFlagsEnum.CRYPT_MACHINE_KEYSET) != 0)
                    cngKeyFlag = (uint)SMASecurity.NativeMethods.NCryptDeletKeyFlag.NCRYPT_MACHINE_KEY_FLAG;
                if (hWnd == IntPtr.Zero ||
                    (keyProvInfo.dwFlags & (uint)SMASecurity.NativeMethods.ProviderFlagsEnum.CRYPT_SILENT) != 0)
                    cngKeyFlag |= (uint)SMASecurity.NativeMethods.NCryptDeletKeyFlag.NCRYPT_SILENT_FLAG;
                int stat = 0;
                    stat = SMASecurity.NativeMethods.NCryptOpenStorageProvider(
                                    ref hCNGProv,
                    if (stat != 0)
                        ThrowErrorRemoting(stat);
                    stat = SMASecurity.NativeMethods.NCryptOpenKey(
                                        hCNGProv,
                                        ref hCNGKey,
                                        keyProvInfo.dwKeySpec,
                                        cngKeyFlag);
                    if ((cngKeyFlag & (uint)SMASecurity.NativeMethods.NCryptDeletKeyFlag.NCRYPT_SILENT_FLAG) != 0)
                            SMASecurity.NativeMethods.NCryptSetProperty(
                                SMASecurity.NativeMethods.NCRYPT_WINDOW_HANDLE_PROPERTY,
                                sizeof(void*),
                                0); // dwFlags
                    stat = SMASecurity.NativeMethods.NCryptDeleteKey(hCNGKey, 0);
                    hCNGKey = IntPtr.Zero;
                    if (hCNGProv != IntPtr.Zero)
                        result = SMASecurity.NativeMethods.NCryptFreeObject(hCNGProv);
                    if (hCNGKey != IntPtr.Zero)
                        result = SMASecurity.NativeMethods.NCryptFreeObject(hCNGKey);
        /// Delete the cert store; if -DeleteKey is specified, we also delete
        /// the associated private key.
        /// <param name="storeName">The store name.</param>
        /// <param name="fDeleteKey">Boolean to specify whether or not to delete private key.</param>
        /// <param name = "sourcePath">Source path.</param>
        private void RemoveCertStore(string storeName, bool fDeleteKey, string sourcePath)
            // if recurse is true, remove every cert in the store
            IntPtr localName = SMASecurity.NativeMethods.CryptFindLocalizedName(storeName);
            string[] pathElements = GetPathElements(sourcePath);
            if (localName == IntPtr.Zero)//not find, we can remove
                X509NativeStore store = null;
                // first open the store
                store = GetStore(sourcePath, false, pathElements);
                // enumerate over each cert and remove it
                while (certContext != IntPtr.Zero)
                    string certPath = sourcePath + cert.Thumbprint;
                    RemoveCertItem(cert, fDeleteKey, true, certPath);
                    certContext = store.GetNextCert(certContext);
                // remove the cert store
                        SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_READONLY_FLAG |
                        SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG |
                        SMASecurity.NativeMethods.CertOpenStoreFlags.CERT_STORE_DELETE_FLAG |
                // delete store
                                storeName);
                                        CertificateProviderStrings.RemoveStoreTemplate,
                const string errorId = "CannotRemoveSystemStore";
        /// Delete the a single cert from the store; if -DeleteKey is specified, we also delete
        /// <param name="cert">An X509Certificate2 object.</param>
        /// <param name="fMachine">Machine context or user.</param>
        private void RemoveCertItem(X509Certificate2 cert, bool fDeleteKey, bool fMachine, string sourcePath)
            if (cert != null)
                string action = null;
                if (fDeleteKey)
                    action = CertificateProviderStrings.Action_RemoveAndDeleteKey;
                    action = CertificateProviderStrings.Action_Remove;
                                        CertificateProviderStrings.RemoveItemTemplate,
                                        sourcePath);
                    DoRemove(cert, fDeleteKey, fMachine, sourcePath);
        /// Delete the cert from the store; if -DeleteKey is specified, we also delete
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private void DoRemove(X509Certificate2 cert, bool fDeleteKey, bool fMachine, string sourcePath)
            // get CERT_KEY_PROV_INFO_PROP_ID
            int provSize = 0;
            IntPtr pProvInfo = IntPtr.Zero;
            bool fHasPrivateKey = false;
                    // it is fine if below call fails
                    if (SMASecurity.NativeMethods.CertGetCertificateContextProperty(
                                cert.Handle,
                                SMASecurity.NativeMethods.CertPropertyId.CERT_KEY_PROV_INFO_PROP_ID,
                                ref provSize))
                        pProvInfo = Marshal.AllocHGlobal((int)provSize);
                                pProvInfo,
                            fHasPrivateKey = true;
                    if (!fHasPrivateKey)
                        // raise a verbose message
                        // we should not use WriteWarning here
                        string verboseNoPrivatekey = CertificateProviderStrings.VerboseNoPrivateKey;
                        WriteVerbose(verboseNoPrivatekey);
                // do remove certificate
                // should not use the original handle
                if (!SMASecurity.NativeMethods.CertDeleteCertificateFromStore(
                            SMASecurity.NativeMethods.CertDuplicateCertificateContext(cert.Handle)))
                // commit the change to physical store
                if (sourcePath.Contains("UserDS"))
                    SMASecurity.NativeMethods.CERT_CONTEXT context =
                        Marshal.PtrToStructure<SMASecurity.NativeMethods.CERT_CONTEXT>(cert.Handle);
                    CommitUserDS(context.hCertStore);
                // TODO: Log Cert Delete
                // delete private key
                if (fDeleteKey && fHasPrivateKey)
                    DoDeleteKey(pProvInfo);
                if (pProvInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(pProvInfo);
        /// Commit store for UserDS store.
        /// <param name="storeHandle">An IntPtr for store handle.</param>
        private static void CommitUserDS(IntPtr storeHandle)
                                        storeHandle,
                                        SMASecurity.NativeMethods.CertControlStoreType.CERT_STORE_CTRL_COMMIT,
        /// Delete the cert from the original store and add to the destination store.
        /// <param name="destination">Destination path.</param>
        /// <param name="cert">An X509Certificate2.</param>
        /// <param name="store">An X509NativeStore.</param>
        /// <param name="sourcePath">Source path.</param>
        private void DoMove(string destination, X509Certificate2 cert, X509NativeStore store, string sourcePath)
            IntPtr dupCert = IntPtr.Zero;  // should not free this
            IntPtr outCert = IntPtr.Zero;
            // duplicate cert first
            dupCert = SMASecurity.NativeMethods.CertDuplicateCertificateContext(cert.Handle);
            if (dupCert == IntPtr.Zero)
                if (!SMASecurity.NativeMethods.CertAddCertificateContextToStore(
                                             store.StoreHandle,
                                             (uint)SMASecurity.NativeMethods.AddCertificateContext.CERT_STORE_ADD_ALWAYS,
                                             ref outCert))
                if (!SMASecurity.NativeMethods.CertDeleteCertificateFromStore(dupCert))
                // TODO: log cert move
            if (destination.Contains("UserDS"))
                CommitUserDS(store.StoreHandle);
                SMASecurity.NativeMethods.CERT_CONTEXT context = Marshal.PtrToStructure<SMASecurity.NativeMethods.CERT_CONTEXT>(cert.Handle);
            // get the output object
            X509Certificate2 outObj = new(outCert);
            string certName = GetCertName(outObj);
            string certPath = MakePath(destination, certName);
            WriteItemObject((object)outObj, certPath, false);
        /// Fetches the store-location/store/certificate at the
        /// specified path.
        /// <param name="path">Path to the item.</param>
        /// <param name="test">True if this is to only for an ItemExists call. Returns True / False.</param>
        /// <param name="isContainer">Set to true if item exists and is a container.</param>
        /// <returns>Item at the path.</returns>
        private object GetItemAtPath(string path, bool test, out bool isContainer)
            // certs have a fixed depth hierarchy.
            // pathElements.Length == 0 ==> List<X509StoreLocation>
            // pathElements.Length == 1 ==> X509StoreLocation
            // pathElements.Length == 2 ==> X509NativeStore
            // pathElements.Length == 3 ==> X509Certificate2
            // Thus lengths 1 & 2 are container items.
            isContainer = pathElements.Length <= 2;
            // handle invalid path depth
            if (pathElements.Length > 3)
                if (test)
            // if path cache already has the item, return it
            item = GetCachedItem(path);
                switch (pathElements.Length)
                        // if this is a single element path and if we
                        // did not find in path-cache, the path
                        // must be wrong. This is because we initialize
                        // the only possible two store locations in ctor
                            isContainer = false;
                            ThrowItemNotFound(path, CertificateProviderItem.StoreLocation);
                        // items at paths of depth 2 are stores.
                        // GetStore() handles store-not-found case. If Test is true,
                        // Item is True / False and we can return it.
                        store = GetStore(path, test, pathElements);
                        item = store;
                        // items at paths of depth 3 are certificates.
                        string storePath = GetParentPath(path, string.Empty);
                        string[] storePathElements = GetPathElements(storePath);
                        // first get the store
                        store = GetStore(storePath, false, storePathElements);
                        // store must be opened to get access to the
                        // certificates within it.
                        IntPtr certContext = store.GetCertByName(pathElements[2]);
                        // Return true / false rather than the certificate
                            item = true;
                            item = new X509Certificate2(certContext);
                        // already handled by ThrowItemNotFound()
                        // at the beginning.
            if ((item != null) && test)
        /// Gets the child items of a given store, or location.
        /// The full path of the store or location to enumerate.
        /// If true, recursively enumerates the child items as well.
        /// Path is null or empty.
        protected override void GetChildItems(string path, bool recurse)
            GetChildItemsOrNames(path, recurse, ReturnContainers.ReturnAllContainers, false, GetFilter());
        /// Gets the child names of a given store, or location.
        /// <param name="returnContainers">
        /// Determines if all containers should be returned or only those containers that match the
        /// filter(s).
        protected override void GetChildNames(
            ReturnContainers returnContainers)
            GetChildItemsOrNames(path, false, returnContainers, true, GetFilter());
        /// Determines if the item at the specified path is a store
        /// or location.
        /// True if the item at the specified path is a store or location.
        /// False otherwise.
        protected override bool IsItemContainer(string path)
                // root path is always container
                isContainer = true;
                // We fetch the item to see if it is a container. This is
                GetItemAtPath(path, true, out isContainer);
            s_tracer.WriteLine("result = {0}", isContainer);
            return isContainer;
        /// Gets the dynamic parameters for get-item on the Certificate
        /// Provider.  We currently support the following dynamic parameters:
        /// "CodeSigning," that returns only certificates good for signing
        /// code or scripts.
        protected override object GetItemDynamicParameters(string path)
            return new CertificateProviderDynamicParameters();
        /// Gets the dynamic parameters for get-childitem on the Certificate
        protected override object GetChildItemsDynamicParameters(string path, bool recurse)
        #endregion DriveCmdletProvider overrides
        /// Helper function to get store-location/store/cert at
        /// the specified path.
        /// <param name="recurse">Whether we need to recursively find all.</param>
        /// <param name="returnNames">Whether we only need the names.</param>
        /// <param name="filter">Filter info.</param>
        /// <returns> Does not return a value.</returns>
        private void GetChildItemsOrNames(
            bool recurse,
            ReturnContainers returnContainers,
            bool returnNames,
            CertificateFilterInfo filter)
            object thingToReturn = null;
            string childPath = null;
            bool returnAllContainers = returnContainers == ReturnContainers.ReturnAllContainers;
            // children at the root path are store locations
                foreach (X509StoreLocation l in s_storeLocations)
                    thingToReturn = returnNames ?
                        (object)l.LocationName : (object)l;
                    // 'returnNames' is true only when called from
                    // GetChildNames(), in which case 'recurse' will always be
                    // false.  When the -Path parameter needs to be globbed,
                    // the potential location names should be returned by
                    // calling this method from GetChildNames.
                    // The original code didn't have a "|| returnNames" clause.
                    // Suppose the user types:
                    //     dir cert:\curr* -CodeSigningCert -recurse
                    // We need to do path globbing here to resolve wild cards.
                    // Since -CodeSigningCert is present, 'filter' is not null.
                    // Since this method is called from GetChildNames() when
                    // doing the path globbing, 'returnNames' is true and
                    // 'recurse' is false.
                    // In the original code, nothing was returned by
                    // WriteItemObject(), so the path globbing fails and the
                    // above dir command would not display the certificates
                    // as expected.
                    // Another case is:
                    //     dir cert:\ -CodeSigningCert -Recurse
                    // -Recurse is present, so we need to call
                    // DoManualGetChildItems, and inside DoManualGetChildItems,
                    // this method will be called to get the names.
                    // The original code had the same problem for this case.
                    // With the "|| returnNames" clause, we test if this method
                    // is called from the GetChildNames().  When this method is
                    // called from GetChildNames(), 'recurse' will always be
                    // false.  Then we should return the names whether 'filter'
                    // is null or not.
                    if (filter == null || returnNames)
                        WriteItemObject(thingToReturn, l.LocationName, true);
                    childPath = l.LocationName;
                    if (recurse)
                        GetChildItemsOrNames(
                                        childPath,
                                        recurse,
                                        returnContainers,
                                        returnNames,
                // children at depth 1 are stores
                if (pathElements.Length == 1)
                    GetStoresOrNames(pathElements[0],
                // children at depth 2 are certificates
                else if (pathElements.Length == 2)
                    GetCertificatesOrNames(path,
                                           pathElements,
        /// Get the name of the specified certificate.
        /// <param name="cert"></param>
        /// <returns>Cert name .</returns>
        /// <remarks> we use Thumbprint as the name  </remarks>
        private static string GetCertName(X509Certificate2 cert)
            return cert.Thumbprint;
        /// Get cert objects or their name at the specified path.
        /// <param name="path">Path to cert.</param>
        /// <param name="pathElements">Path elements.</param>
        /// <param name="returnNames">Whether we should return only the names (instead of objects).</param>
        /// <returns>Does not return a value.</returns>
        private void GetCertificatesOrNames(string path,
                                             string[] pathElements,
            string certPath = null;
            store = GetStore(path, false, pathElements);
            // enumerate over each cert and return it (or its name)
                    string certName = GetCertName(cert);
                    certPath = MakePath(path, certName);
                    if (returnNames)
                        thingToReturn = (object)certName;
                        PSObject myPsObj = new(cert);
                        thingToReturn = (object)myPsObj;
                    WriteItemObject(thingToReturn, certPath, false);
        /// Get X509StoreLocation object at path.
        /// <returns>X509StoreLocation object.</returns>
        private X509StoreLocation GetStoreLocation(string path)
            // we store the only two possible store-location
            // objects during ctor.
            X509StoreLocation location =
                GetCachedItem(path) as X509StoreLocation;
            if (location == null)
        /// Get the X509NativeStore object at path.
        /// <param name="path">Path to store.</param>
        /// <param name="test">True if this should be a test for path existence. Returns True or False.</param>
        /// <returns>X509NativeStore object.</returns>
        private X509NativeStore GetStore(string path, bool test, string[] pathElements)
            X509StoreLocation location = GetStoreLocation(pathElements[0]);
            X509NativeStore store = GetStore(path, pathElements[1], location);
            if (store == null)
                    ThrowItemNotFound(path, CertificateProviderItem.Store);
            return store;
        /// Gets the X509NativeStore at the specified path.
        /// Adds to cache if not already there.
        /// <param name="storePath">Path to the store.</param>
        /// <param name="storeName">Name of store (path leaf element).</param>
        /// <param name="storeLocation">Location of store (CurrentUser or LocalMachine).</param>
        private X509NativeStore GetStore(string storePath,
                                   string storeName,
                                   X509StoreLocation storeLocation)
            if (!storeLocation.StoreNames.ContainsKey(storeName))
                ThrowItemNotFound(storePath, CertificateProviderItem.Store);
            if (s_storeCache != null)
                if (s_storeCache.Location != storeLocation ||
                    !string.Equals(
                                s_storeCache.StoreName,
                                storeName,
                    s_storeCache = null;
            s_storeCache ??= new X509NativeStore(storeLocation, storeName);
            return s_storeCache;
        /// Gets X509NativeStore objects or their name at the specified path.
        /// <param name="path">Path to the store.</param>
        /// <param name="recurse">Recursively return all items if true.</param>
        /// <param name="returnNames"></param>
        private void GetStoresOrNames(
            X509StoreLocation location = GetStoreLocation(path);
            string storePath = null;
            // enumerate over each store
            foreach (string name in location.StoreNames.Keys)
                storePath = MakePath(path, name);
                    thingToReturn = name;
                    X509NativeStore store = GetStore(storePath, name, location);
                    X509Store ManagedStore = new(store.StoreName, store.Location.Location);
                    thingToReturn = ManagedStore;
                // the potential store names should be returned by
                //     dir cert:\CurrentUser\Tru* -CodeSigningCert -recurse
                //     dir cert:\CurrentUser -CodeSigningCert -Recurse
                    WriteItemObject(thingToReturn, name, true);
                // if recurse is true, get cert objects (or names) as well
                    string[] pathElements = GetPathElements(storePath);
                    GetCertificatesOrNames(
                                    storePath,
        private CertificateFilterInfo GetFilter()
            CertificateFilterInfo filter = null;
            if (DynamicParameters != null && DynamicParameters is CertificateProviderDynamicParameters dp)
                if (dp.CodeSigningCert)
                    filter = new CertificateFilterInfo();
                    filter.Purpose = CertificatePurpose.CodeSigning;
                if (dp.DocumentEncryptionCert)
                    filter ??= new CertificateFilterInfo();
                    filter.Purpose = CertificatePurpose.DocumentEncryption;
                if (dp.DnsName != null)
                    filter.DnsName = new WildcardPattern(dp.DnsName, WildcardOptions.IgnoreCase);
                if (dp.Eku != null)
                    filter.Eku = new List<WildcardPattern>();
                    foreach (var pattern in dp.Eku)
                        filter.Eku.Add(new WildcardPattern(pattern, WildcardOptions.IgnoreCase));
                if (dp.ExpiringInDays >= 0)
                    filter.Expiring = DateTime.Now.AddDays(dp.ExpiringInDays);
                if (dp.SSLServerAuthentication)
                    filter.SSLServerAuthentication = true;
        private bool IncludeArchivedCerts()
            bool includeArchivedCerts = false;
                includeArchivedCerts = true;
            return includeArchivedCerts;
        private static bool MatchesFilter(X509Certificate2 cert, CertificateFilterInfo filter)
            // No filter means, match everything
            if (filter.Expiring > DateTime.MinValue && !SecuritySupport.CertExpiresByTime(cert, filter.Expiring))
            if (filter.DnsName != null && !CertContainsName(cert, filter.DnsName))
            if (filter.Eku != null && !CertContainsEku(cert, filter.Eku))
            if (filter.SSLServerAuthentication && !CertIsSSLServerAuthentication(cert))
            switch (filter.Purpose)
                case CertificatePurpose.CodeSigning:
                    return SecuritySupport.CertIsGoodForSigning(cert);
                case CertificatePurpose.DocumentEncryption:
                    return SecuritySupport.CertIsGoodForEncryption(cert);
                case CertificatePurpose.NotSpecified:
                case CertificatePurpose.All:
        /// Check if the specified certificate has the name in DNS name list.
        /// <param name="cert">Certificate object.</param>
        /// <param name="pattern">Wildcard pattern for DNS name to search.</param>
        /// <returns>True on success, false otherwise.</returns>
        internal static bool CertContainsName(X509Certificate2 cert, WildcardPattern pattern)
            List<DnsNameRepresentation> list = (new DnsNameProperty(cert)).DnsNameList;
            foreach (DnsNameRepresentation dnsName in list)
                if (pattern.IsMatch(dnsName.Unicode))
        /// Check if the specified certificate is a server authentication certificate.
        internal static bool CertIsSSLServerAuthentication(X509Certificate2 cert)
            X509ExtensionCollection extentionList = cert.Extensions;
            foreach (var extension in extentionList)
                if (extension is X509EnhancedKeyUsageExtension eku)
                    foreach (Oid usage in eku.EnhancedKeyUsages)
                        if (usage.Value.Equals(CertificateFilterInfo.OID_PKIX_KP_SERVER_AUTH, StringComparison.Ordinal))
        /// Check if the specified certificate contains EKU matching all of these patterns.
        /// <param name="ekuPatterns">EKU patterns.</param>
        internal static bool CertContainsEku(X509Certificate2 cert, List<WildcardPattern> ekuPatterns)
            X509ExtensionCollection extensionList = cert.Extensions;
            foreach (var extension in extensionList)
                    OidCollection enhancedKeyUsages = eku.EnhancedKeyUsages;
                    foreach (WildcardPattern ekuPattern in ekuPatterns)
                        const bool patternPassed = false;
                        foreach (var usage in enhancedKeyUsages)
                            if (ekuPattern.IsMatch(usage.Value) || ekuPattern.IsMatch(usage.FriendlyName))
                        if (!patternPassed)
        private static object GetCachedItem(string path)
                if (s_pathCache.ContainsKey(path))
                    item = s_pathCache[path];
                    Dbg.Diagnostics.Assert(item != null, "GetCachedItem");
        private static void AddItemToCache(string path, object item)
                if ((item != null) && (!s_pathCache.ContainsKey(path)))
                    s_pathCache.Add(path, item);
        #endregion private members
        #region ICmdletProviderSupportsHelp Members
        /// Get provider-specific help.
        /// <param name="helpItemName">
        /// Name of help item or cmdlet for which user has requested help
        /// <param name = "path">
        /// Path to the current location or path to the location of the property that the user needs
        /// help about.
        /// Provider specific MAML help content string
        string ICmdletProviderSupportsHelp.GetHelpMaml(string helpItemName, string path)
            // Get the ver and noun from helpItemName
            string verb = null;
            string noun = null;
                if (!string.IsNullOrEmpty(helpItemName))
                    CmdletInfo.SplitCmdletName(helpItemName, out verb, out noun);
                if (string.IsNullOrEmpty(verb) || string.IsNullOrEmpty(noun))
                // Load the help file from the current UI culture subfolder of the module's root folder
                XmlDocument document = new();
                CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
                string fullHelpPath = Path.Combine(
                    this.ProviderInfo.ApplicationBase,
                    currentUICulture.ToString(),
                    this.ProviderInfo.HelpFile);
                XmlReaderSettings settings = new();
                settings.XmlResolver = null;
                using (XmlReader reader = XmlReader.Create(fullHelpPath, settings))
                    document.Load(reader);
                // Add "msh" and "command" namespaces from the MAML schema
                XmlNamespaceManager nsMgr = new(document.NameTable);
                nsMgr.AddNamespace("msh", HelpCommentsParser.mshURI);
                nsMgr.AddNamespace("command", HelpCommentsParser.commandURI);
                // Compose XPath query to select the appropriate node based on the cmdlet
                string xpathQuery = string.Format(
                    HelpCommentsParser.ProviderHelpCommandXPath,
                    verb,
                    noun);
                // Execute the XPath query and return its MAML snippet
                XmlNode result = document.SelectSingleNode(xpathQuery, nsMgr);
                    return result.OuterXml;
            catch (PathTooLongException)
            catch (IOException)
            catch (SecurityException)
            catch (XPathException)
    /// Defines a class to represent a store location in the certificate
    /// provider.  The two possible store locations are CurrentUser and
    /// LocalMachine.
    public sealed class X509StoreLocation
        /// Gets the location, as a string.
        public string LocationName
                return _location.ToString();
        /// Gets the location as a
        /// <see cref="System.Security.Cryptography.X509Certificates.StoreLocation"/>
        public StoreLocation Location
                return _location;
                _location = value;
        private StoreLocation _location = StoreLocation.CurrentUser;
        /// Gets the list of stores at this location.
        public Hashtable StoreNames
                Hashtable storeNames;
                // always try to get new names
                storeNames = new Hashtable(StringComparer.OrdinalIgnoreCase);
                // since there is no managed support to obtain store names,
                // we use pinvoke to get it ourselves.
                List<string> names = Crypt32Helpers.GetStoreNamesAtLocation(_location);
                    storeNames.Add(name, true);
                return storeNames;
        /// Initializes a new instance of the X509StoreLocation class.
        public X509StoreLocation(StoreLocation location)
            Location = location;
    /// Defines the type of EKU string
    /// The structure contains friendly name and EKU oid.
    public readonly struct EnhancedKeyUsageRepresentation
        /// Localized friendly name of EKU.
        private readonly string _friendlyName;
        /// OID of EKU.
        private readonly string _oid;
        /// Constructor of an EnhancedKeyUsageRepresentation.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Oid")]
        public EnhancedKeyUsageRepresentation(string inputFriendlyName, string inputOid)
            _friendlyName = inputFriendlyName;
            _oid = inputOid;
        public bool Equals(EnhancedKeyUsageRepresentation keyUsage)
            if (_oid != null && keyUsage._oid != null)
                // OID strings only contain numbers and periods
                if (string.Equals(_oid, keyUsage._oid, StringComparison.Ordinal))
            else if (_oid == null && keyUsage._oid == null)
        /// Get property of friendlyName.
        public string FriendlyName
                return _friendlyName;
        /// Get property of oid.
        public string ObjectId
                return _oid;
            return string.IsNullOrEmpty(_friendlyName) ?
                        _oid :
                        _friendlyName + " (" + _oid + ")";
    /// Class for SendAsTrustedIssuer.
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public sealed class SendAsTrustedIssuerProperty
        /// Get property of SendAsTrustedIssuer.
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static bool ReadSendAsTrustedIssuerProperty(X509Certificate2 cert)
            bool fHasProperty = false;
            if (DownLevelHelper.TrustedIssuerSupported())
                int propSize = 0;
                // try to get the property
                // it is fine if fail for not there
                                SMASecurity.NativeMethods.CertPropertyId.CERT_SEND_AS_TRUSTED_ISSUER_PROP_ID,
                                ref propSize))
                    // we have the property
                    fHasProperty = true;
                    // if fail
                    if (error != SMASecurity.NativeMethods.CRYPT_E_NOT_FOUND)
                        throw new System.ComponentModel.Win32Exception(error);
            return fHasProperty;
        /// Set property of SendAsTrustedIssuer.
        public static void WriteSendAsTrustedIssuerProperty(X509Certificate2 cert, string certPath, bool addProperty)
                IntPtr propertyPtr = IntPtr.Zero;
                SMASecurity.NativeMethods.CRYPT_DATA_BLOB dataBlob = new();
                dataBlob.cbData = 0;
                dataBlob.pbData = IntPtr.Zero;
                X509Certificate certFromStore = null;
                    if (certPath != null)
                        // try to open the store and get the cert out
                        // in case the store handle is already released
                        string[] pathElements = GetPathElements(certPath);
                        // certpath is in the format: Microsoft.Powershell.Security\
                        // Certificate::CurrentUser(LocalMachine)\my\HashID
                        // obtained pathElements[0] is Microsoft.Powershell.Security
                        // obtained pathElements[1] is Certificate::CurrentUser
                        // obtained pathElements[2] is MY
                        // obtained pathElements[3] is HashID
                        bool fUserContext = string.Equals(pathElements[1], "Certificate::CurrentUser", StringComparison.OrdinalIgnoreCase);
                        X509StoreLocation storeLocation =
                            new(fUserContext ? StoreLocation.CurrentUser : StoreLocation.LocalMachine);
                        // get certificate from the store pathElements[2]
                        store = new X509NativeStore(storeLocation, pathElements[2]);
                        store.Open(true); // including archival flag
                        IntPtr certContext = store.GetCertByName(pathElements[3]);
                            certFromStore = new X509Certificate2(certContext);
                    if (addProperty) // should add the property
                        propertyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(dataBlob));
                        Marshal.StructureToPtr(dataBlob, propertyPtr, false);
                    // set property
                    if (!SMASecurity.NativeMethods.CertSetCertificateContextProperty(
                                certFromStore != null ? certFromStore.Handle : cert.Handle,
                                propertyPtr))
                    if (propertyPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(propertyPtr);
                Marshal.ThrowExceptionForHR(SMASecurity.NativeMethods.NTE_NOT_SUPPORTED);
        private static readonly char[] s_separators = new char[] { '/', '\\' };
            string[] allElts = path.Split(s_separators);
    /// Class for ekulist.
    public sealed class EnhancedKeyUsageProperty
        private readonly List<EnhancedKeyUsageRepresentation> _ekuList = new();
        /// Get property of EKUList.
        public List<EnhancedKeyUsageRepresentation> EnhancedKeyUsageList
                return _ekuList;
        /// Constructor for EnhancedKeyUsageProperty.
        public EnhancedKeyUsageProperty(X509Certificate2 cert)
            foreach (X509Extension extension in cert.Extensions)
                // Filter to the OID for EKU
                if (extension.Oid.Value == "2.5.29.37" && extension is X509EnhancedKeyUsageExtension ext)
                    OidCollection oids = ext.EnhancedKeyUsages;
                    foreach (Oid oid in oids)
                        EnhancedKeyUsageRepresentation ekuString = new(oid.FriendlyName, oid.Value);
                        _ekuList.Add(ekuString);
    /// Class for DNSNameList.
    public sealed class DnsNameProperty
        private readonly List<DnsNameRepresentation> _dnsList = new();
        private readonly IdnMapping idnMapping = new();
        private const string distinguishedNamePrefix = "CN=";
        /// Get property of DnsNameList.
        public List<DnsNameRepresentation> DnsNameList => _dnsList;
        private DnsNameRepresentation GetDnsNameRepresentation(string dnsName)
            string unicodeName;
                unicodeName = idnMapping.GetUnicode(dnsName);
                // The name is not valid Punycode, assume it's valid ASCII.
                unicodeName = dnsName;
            return new DnsNameRepresentation(dnsName, unicodeName);
        /// Constructor for DnsNameProperty.
        public DnsNameProperty(X509Certificate2 cert)
            _dnsList = new List<DnsNameRepresentation>();
            // extract DNS name from subject distinguish name
            // if it exists and does not contain a comma
            // a comma, indicates it is not a DNS name
            if (cert.Subject.StartsWith(distinguishedNamePrefix, StringComparison.OrdinalIgnoreCase) &&
                !cert.Subject.Contains(','))
                string parsedSubjectDistinguishedDnsName = cert.Subject.Substring(distinguishedNamePrefix.Length);
                DnsNameRepresentation dnsName = GetDnsNameRepresentation(parsedSubjectDistinguishedDnsName);
                _dnsList.Add(dnsName);
            // Extract DNS names from SAN extensions
                if (extension is X509SubjectAlternativeNameExtension sanExtension)
                    foreach (string dnsNameEntry in sanExtension.EnumerateDnsNames())
                        DnsNameRepresentation dnsName = GetDnsNameRepresentation(dnsNameEntry);
                        // Only add the name if it is not the same as an existing name.
                        if (!_dnsList.Contains(dnsName))
    /// Downlevel helper function to determine if the OS is WIN8 and above.
    internal static class DownLevelHelper
        private static bool s_isWin8Set = false;
        private static bool s_isWin8 = false;
        internal static bool IsWin8AndAbove()
            if (!s_isWin8Set)
                    ((version.Major > 6) ||
                     (version.Major == 6 && version.Minor >= 2)))
                    s_isWin8 = true;
                s_isWin8Set = true;
            return s_isWin8;
        internal static bool TrustedIssuerSupported()
            return IsWin8AndAbove();
        internal static bool HashLookupSupported()
    /// Check in UI is allowed.
    internal static class DetectUIHelper
        internal static IntPtr GetOwnerWindow(PSHost host)
        private static IntPtr hWnd = IntPtr.Zero;
        private static bool firstRun = true;
            if (firstRun)
                firstRun = false;
                if (IsUIAllowed(host))
                    hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                    if (hWnd == IntPtr.Zero)
                        hWnd = SMASecurity.NativeMethods.GetConsoleWindow();
                        hWnd = SMASecurity.NativeMethods.GetDesktopWindow();
            return hWnd;
        private static bool IsUIAllowed(PSHost host)
            if (host.Name.Equals("ServerRemoteHost", StringComparison.OrdinalIgnoreCase))
            uint SessionId;
            if (!SMASecurity.NativeMethods.ProcessIdToSessionId((uint)Environment.ProcessId, out SessionId))
            if (SessionId == 0)
            if (!Environment.UserInteractive)
            string[] args = Environment.GetCommandLineArgs();
            bool fRet = true;
            foreach (string arg in args)
                const string NonInteractiveParamName = "-noninteractive";
                if (arg.Length >= 4 && NonInteractiveParamName.StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                    fRet = false;
            return fRet;
    /// Container for helper functions that use pinvoke into crypt32.dll.
    internal static class Crypt32Helpers
        /// -- storeNames.
        internal static readonly List<string> storeNames = new();
        /// Get a list of store names at the specified location.
        internal static List<string> GetStoreNamesAtLocation(StoreLocation location)
            SMASecurity.NativeMethods.CertStoreFlags locationFlag =
                SMASecurity.NativeMethods.CertStoreFlags.CERT_SYSTEM_STORE_CURRENT_USER;
            switch (location)
                    locationFlag = SMASecurity.NativeMethods.CertStoreFlags.CERT_SYSTEM_STORE_CURRENT_USER;
                    locationFlag = SMASecurity.NativeMethods.CertStoreFlags.CERT_SYSTEM_STORE_LOCAL_MACHINE;
                    Diagnostics.Assert(false, "GetStoreNamesAtLocation: invalid location value");
            SMASecurity.NativeMethods.CertEnumSystemStoreCallBackProto callBack = new(CertEnumSystemStoreCallBack);
            // Return a new list to avoid synchronization issues.
            List<string> names = new();
                storeNames.Clear();
                SMASecurity.NativeMethods.CertEnumSystemStore(locationFlag, IntPtr.Zero,
                                                  IntPtr.Zero, callBack);
                foreach (string name in storeNames)
                    names.Add(name);
            return names;
        /// Call back function used by CertEnumSystemStore
        /// Currently, there is no managed support for enumerating store
        /// names on a machine. We use the win32 function CertEnumSystemStore()
        /// to get a list of stores for a given context.
        /// Each time this callback is called, we add the passed store name
        /// to the list of stores.
        internal static bool CertEnumSystemStoreCallBack(string storeName,
                                                          DWORD dwFlagsNotUsed,
                                                          IntPtr notUsed1,
                                                          IntPtr notUsed2,
                                                          IntPtr notUsed3)
            storeNames.Add(storeName);

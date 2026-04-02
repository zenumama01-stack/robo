    #region WSManCredSSP cmdlet base
    /// Base class used *-WSManCredSSP cmdlets (Enable-WSManCredSSP, Disable-WSManCredSSP)
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cred")]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SSP")]
    public class WSManCredSSPCommandBase : PSCmdlet
        #region Protected / Internal Data
        internal const string Server = "Server";
        internal const string Client = "Client";
        /// Role can either "Client" or "Server".
        [ValidateSet(Client, Server)]
        public string Role
            get { return role; }
            set { role = value; }
        private string role;
        #region Utilities
        /// Returns a session object upon successful creation..otherwise
        /// writes an error using WriteError and returns null.
        internal IWSManSession CreateWSManSession()
            IWSManEx wsmanObject = (IWSManEx)new WSManClass();
            IWSManSession m_SessionObj = null;
                m_SessionObj = (IWSManSession)wsmanObject.CreateSession(null, 0, null);
                return m_SessionObj;
            catch (COMException ex)
                ErrorRecord er = new ErrorRecord(ex, "COMException", ErrorCategory.InvalidOperation, null);
    #region DisableWsManCredSsp
    /// Disables CredSSP authentication on the client. CredSSP authentication
    /// enables an application to delegate the user's credentials from the client to
    /// the server, hence allowing the user to perform management operations that
    /// access a second hop.
    [Cmdlet(VerbsLifecycle.Disable, "WSManCredSSP", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096628")]
    public class DisableWSManCredSSPCommand : WSManCredSSPCommandBase, IDisposable
        // The application name MUST be "wsman" as wsman got approval from security
        // folks who suggested to register the SPN with name "wsman".
        private const string applicationname = "wsman";
        private void DisableClientSideSettings()
            WSManHelper helper = new WSManHelper(this);
            IWSManSession m_SessionObj = CreateWSManSession();
            if (m_SessionObj == null)
                string result = m_SessionObj.Get(helper.CredSSP_RUri, 0);
                XmlDocument resultopxml = new XmlDocument();
                resultopxml.LoadXml(result);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(resultopxml.NameTable);
                nsmgr.AddNamespace("cfg", helper.CredSSP_XMLNmsp);
                XmlNode xNode = resultopxml.SelectSingleNode(helper.CredSSP_SNode, nsmgr);
                if (xNode is null)
                    InvalidOperationException ex = new InvalidOperationException();
                    ErrorRecord er = new ErrorRecord(ex, helper.GetResourceMsgFromResourcetext("WinrmNotConfigured"), ErrorCategory.InvalidOperation, null);
                string inputXml = @"<cfg:Auth xmlns:cfg=""http://schemas.microsoft.com/wbem/wsman/1/config/client/auth""><cfg:CredSSP>false</cfg:CredSSP></cfg:Auth>";
                m_SessionObj.Put(helper.CredSSP_RUri, inputXml, 0);
                    this.DeleteUserDelegateSettings();
                    ThreadStart start = new ThreadStart(this.DeleteUserDelegateSettings);
                    Thread thread = new Thread(start);
                if (!helper.ValidateCreadSSPRegistryRetry(false, null, applicationname))
                    helper.AssertError(helper.GetResourceMsgFromResourcetext("DisableCredSSPPolicyValidateError"), false, null);
                ErrorRecord er = new ErrorRecord(ex, "XpathException", ErrorCategory.InvalidOperation, null);
                if (!string.IsNullOrEmpty(m_SessionObj.Error))
                    helper.AssertError(m_SessionObj.Error, true, null);
                if (m_SessionObj != null)
                    Dispose(m_SessionObj);
        private void DisableServerSideSettings()
                string result = m_SessionObj.Get(helper.Service_CredSSP_Uri, 0);
                nsmgr.AddNamespace("cfg", helper.Service_CredSSP_XMLNmsp);
                    ErrorRecord er = new ErrorRecord(ex,
                        helper.GetResourceMsgFromResourcetext("WinrmNotConfigured"),
                        ErrorCategory.InvalidOperation, null);
                string inputXml = string.Format(
                    @"<cfg:Auth xmlns:cfg=""{0}""><cfg:CredSSP>false</cfg:CredSSP></cfg:Auth>",
                    helper.Service_CredSSP_XMLNmsp);
                m_SessionObj.Put(helper.Service_CredSSP_Uri, inputXml, 0);
        private void DeleteUserDelegateSettings()
            System.IntPtr KeyHandle = System.IntPtr.Zero;
            IGroupPolicyObject GPO = (IGroupPolicyObject)new GPClass();
            GPO.OpenLocalMachineGPO(1);
            KeyHandle = GPO.GetRegistryKey(2);
            RegistryKey rootKey = Registry.CurrentUser;
            const string GPOpath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy Objects";
            RegistryKey GPOKey = rootKey.OpenSubKey(GPOpath, true);
            foreach (string keyname in GPOKey.GetSubKeyNames())
                if (keyname.EndsWith("Machine", StringComparison.OrdinalIgnoreCase))
                    string key = GPOpath + "\\" + keyname + "\\" + @"Software\Policies\Microsoft\Windows";
                    DeleteDelegateSettings(applicationname, Registry.CurrentUser, key, GPO);
            KeyHandle = System.IntPtr.Zero;
        private void DeleteDelegateSettings(string applicationname, RegistryKey rootKey, string Registry_Path, IGroupPolicyObject GPO)
            RegistryKey rKey;
            bool otherkeys = false;
                string Registry_Path_Credentials_Delegation = Registry_Path + @"\CredentialsDelegation";
                RegistryKey Allow_Fresh_Credential_Key = rootKey.OpenSubKey(Registry_Path_Credentials_Delegation + @"\" + helper.Key_Allow_Fresh_Credentials, true);
                if (Allow_Fresh_Credential_Key != null)
                    string[] valuenames = Allow_Fresh_Credential_Key.GetValueNames();
                    if (valuenames.Length > 0)
                        Collection<string> KeyCollection = new Collection<string>();
                        foreach (string value in valuenames)
                            object keyvalue = Allow_Fresh_Credential_Key.GetValue(value);
                            if (keyvalue != null)
                                if (!keyvalue.ToString().StartsWith(applicationname, StringComparison.OrdinalIgnoreCase))
                                    KeyCollection.Add(keyvalue.ToString());
                                    otherkeys = true;
                            Allow_Fresh_Credential_Key.DeleteValue(value);
                        foreach (string keyvalue in KeyCollection)
                            Allow_Fresh_Credential_Key.SetValue(Convert.ToString(i + 1, CultureInfo.InvariantCulture), keyvalue, RegistryValueKind.String);
                if (!otherkeys)
                    rKey = rootKey.OpenSubKey(Registry_Path_Credentials_Delegation, true);
                    if (rKey != null)
                        object regval1 = rKey.GetValue(helper.Key_Allow_Fresh_Credentials);
                        if (regval1 != null)
                            rKey.DeleteValue(helper.Key_Allow_Fresh_Credentials, false);
                        object regval2 = rKey.GetValue(helper.Key_Concatenate_Defaults_AllowFresh);
                        if (regval2 != null)
                            rKey.DeleteValue(helper.Key_Concatenate_Defaults_AllowFresh, false);
                        if (rKey.OpenSubKey(helper.Key_Allow_Fresh_Credentials) != null)
                            rKey.DeleteSubKeyTree(helper.Key_Allow_Fresh_Credentials);
                GPO.Save(true, true, new Guid("35378EAC-683F-11D2-A89A-00C04FBBCFA2"), new Guid("6AD20875-336C-4e22-968F-C709ACB15814"));
                ErrorRecord er = new ErrorRecord(ex, "InvalidOperation", ErrorCategory.InvalidOperation, null);
                ErrorRecord er = new ErrorRecord(ex, "InvalidArgument", ErrorCategory.InvalidArgument, null);
            catch (SecurityException ex)
                ErrorRecord er = new ErrorRecord(ex, "SecurityException", ErrorCategory.SecurityError, null);
                ErrorRecord er = new ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.SecurityError, null);
        /// Begin processing method.
            // If not running elevated, then throw an "elevation required" error message.
            if (Role.Equals(Client, StringComparison.OrdinalIgnoreCase))
                DisableClientSideSettings();
            if (Role.Equals(Server, StringComparison.OrdinalIgnoreCase))
                DisableServerSideSettings();
            // CleanUp();
        Dispose(IWSManSession sessionObject)
            sessionObject = null;
    #endregion DisableWsManCredSSP
    #region EnableCredSSP
    /// Enables CredSSP authentication on the client. CredSSP authentication enables
    /// an application to delegate the user's credentials from the client to the
    /// server, hence allowing the user to perform management operations that access
    /// a second hop.
    /// This cmdlet performs the following:
    /// On the client:
    /// 1. Enables WSMan local configuration on client to enable CredSSP
    /// 2. Sets CredSSP policy AllowFreshCredentials to wsman/Delegate. This policy
    /// allows delegating explicit credentials to a server when server
    /// authentication is achieved via a trusted X509 certificate or Kerberos.
    [Cmdlet(VerbsLifecycle.Enable, "WSManCredSSP", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096719")]
    [OutputType(typeof(XmlElement))]
    public class EnableWSManCredSSPCommand : WSManCredSSPCommandBase, IDisposable/*, IDynamicParameters*/
        /// Delegate parameter.
        public string[] DelegateComputer
            get { return delegatecomputer; }
            set { delegatecomputer = value; }
        private string[] delegatecomputer;
            get { return force; }
            set { force = value; }
        private bool force = false;
        // helper variable
        private WSManHelper helper;
        #region Cmdlet Overloads
            // DelegateComputer cannot be specified when Role is other than client
            if ((delegatecomputer != null) && !Role.Equals(Client, StringComparison.OrdinalIgnoreCase))
                string message = helper.FormatResourceMsgFromResourcetext("CredSSPRoleAndDelegateCannotBeSpecified",
                    "DelegateComputer",
                    "Role",
                    Role,
                    Client);
                throw new InvalidOperationException(message);
            // DelegateComputer must be specified when Role is client
            if (Role.Equals(Client, StringComparison.OrdinalIgnoreCase) && (delegatecomputer == null))
                string message = helper.FormatResourceMsgFromResourcetext("CredSSPClientAndDelegateMustBeSpecified",
                EnableClientSideSettings();
                EnableServerSideSettings();
        private void EnableClientSideSettings()
            string query = helper.GetResourceMsgFromResourcetext("CredSSPContinueQuery");
            string caption = helper.GetResourceMsgFromResourcetext("CredSSPContinueCaption");
            if (!force && !ShouldContinue(query, caption))
                // get the credssp node to check if wsman is configured on this machine
                XmlNode node = helper.GetXmlNode(result, helper.CredSSP_SNode, helper.CredSSP_XMLNmsp);
                const string newxmlcontent = @"<cfg:Auth xmlns:cfg=""http://schemas.microsoft.com/wbem/wsman/1/config/client/auth""><cfg:CredSSP>true</cfg:CredSSP></cfg:Auth>";
                    XmlDocument xmldoc = new XmlDocument();
                    // push the xml string with credssp enabled
                    xmldoc.LoadXml(m_SessionObj.Put(helper.CredSSP_RUri, newxmlcontent, 0));
                    // set the Registry using GroupPolicyObject
                        this.UpdateCurrentUserRegistrySettings();
                        ThreadStart start = new ThreadStart(this.UpdateCurrentUserRegistrySettings);
                    if (helper.ValidateCreadSSPRegistryRetry(true, delegatecomputer, applicationname))
                        WriteObject(xmldoc.FirstChild);
                        helper.AssertError(helper.GetResourceMsgFromResourcetext("EnableCredSSPPolicyValidateError"), false, delegatecomputer);
                catch (COMException)
                    helper.AssertError(m_SessionObj.Error, true, delegatecomputer);
        private void EnableServerSideSettings()
            string query = helper.GetResourceMsgFromResourcetext("CredSSPServerContinueQuery");
                XmlNode node = helper.GetXmlNode(result,
                    helper.CredSSP_SNode,
                    string newxmlcontent = string.Format(
                        @"<cfg:Auth xmlns:cfg=""{0}""><cfg:CredSSP>true</cfg:CredSSP></cfg:Auth>",
                    xmldoc.LoadXml(m_SessionObj.Put(helper.Service_CredSSP_Uri, newxmlcontent, 0));
        private void UpdateCurrentUserRegistrySettings()
                    UpdateGPORegistrySettings(applicationname, this.delegatecomputer, Registry.CurrentUser, key);
            // saving gpo settings
            GPO.Save(true, true, new Guid("35378EAC-683F-11D2-A89A-00C04FBBCFA2"), new Guid("7A9206BD-33AF-47af-B832-D4128730E990"));
        /// Updates the grouppolicy registry settings.
        /// <param name="applicationname"></param>
        /// <param name="delegatestring"></param>
        /// <param name="rootKey"></param>
        /// <param name="Registry_Path"></param>
        private void UpdateGPORegistrySettings(string applicationname, string[] delegatestring, RegistryKey rootKey, string Registry_Path)
            // RegistryKey rootKey = Registry.LocalMachine;
            RegistryKey Credential_Delegation_Key;
            RegistryKey Allow_Fresh_Credential_Key;
                // open the registry key.If key is not present,create a new one
                Credential_Delegation_Key = rootKey.OpenSubKey(Registry_Path_Credentials_Delegation, true) ?? rootKey.CreateSubKey(Registry_Path_Credentials_Delegation, RegistryKeyPermissionCheck.ReadWriteSubTree);
                Credential_Delegation_Key.SetValue(helper.Key_Allow_Fresh_Credentials, 1, RegistryValueKind.DWord);
                Credential_Delegation_Key.SetValue(helper.Key_Concatenate_Defaults_AllowFresh, 1, RegistryValueKind.DWord);
                // add the delegate value
                Allow_Fresh_Credential_Key = rootKey.OpenSubKey(Registry_Path_Credentials_Delegation + @"\" + helper.Key_Allow_Fresh_Credentials, true) ?? rootKey.CreateSubKey(Registry_Path_Credentials_Delegation + @"\" + helper.Key_Allow_Fresh_Credentials, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    i = Allow_Fresh_Credential_Key.ValueCount;
                    foreach (string del in delegatestring)
                        Allow_Fresh_Credential_Key.SetValue(Convert.ToString(i + 1, CultureInfo.InvariantCulture), applicationname + @"/" + del, RegistryValueKind.String);
                ErrorRecord er = new ErrorRecord(ex, "UnauthorizedAccessException", ErrorCategory.PermissionDenied, null);
                ErrorRecord er = new ErrorRecord(ex, "SecurityException", ErrorCategory.InvalidOperation, null);
    #endregion EnableCredSSP
    #region Get-CredSSP
    /// Gets the CredSSP related configuration on the client. CredSSP authentication
    /// 1. Gets the configuration for WSMan policy on client to enable/disable
    /// CredSSP
    /// 2. Gets the configuration information for the CredSSP policy
    /// AllowFreshCredentials . This policy allows delegating explicit credentials
    /// to a server when server authentication is achieved via a trusted X509
    /// certificate or Kerberos.
    [Cmdlet(VerbsCommon.Get, "WSManCredSSP", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096838")]
    public class GetWSManCredSSPCommand : PSCmdlet, IDisposable
        private WSManHelper helper = null;
        /// Method to get the values.
        private string GetDelegateSettings(string applicationname)
            RegistryKey rootKey = Registry.LocalMachine;
            string[] valuenames = null;
                string Reg_key = helper.Registry_Path_Credentials_Delegation + @"\CredentialsDelegation";
                rKey = rootKey.OpenSubKey(Reg_key);
                    rKey = rKey.OpenSubKey(helper.Key_Allow_Fresh_Credentials);
                        valuenames = rKey.GetValueNames();
                            string listvalue = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                                object keyvalue = rKey.GetValue(value);
                                    if (keyvalue.ToString().StartsWith(applicationname, StringComparison.OrdinalIgnoreCase))
                                        result = keyvalue.ToString() + listvalue + result;
                            if (result.EndsWith(listvalue, StringComparison.OrdinalIgnoreCase))
                                result = result.Remove(result.Length - 1);
                ErrorRecord er = new ErrorRecord(ex, "ArgumentException", ErrorCategory.PermissionDenied, null);
                ErrorRecord er = new ErrorRecord(ex, "SecurityException", ErrorCategory.PermissionDenied, null);
                ErrorRecord er = new ErrorRecord(ex, "ObjectDisposedException", ErrorCategory.PermissionDenied, null);
        /// Method to begin processing.
                const string applicationname = "wsman";
                string credsspResult = GetDelegateSettings(applicationname);
                if (string.IsNullOrEmpty(credsspResult))
                    WriteObject(helper.GetResourceMsgFromResourcetext("NoDelegateFreshCred"));
                    WriteObject(helper.GetResourceMsgFromResourcetext("DelegateFreshCred") + credsspResult);
                // Get the server side settings
                result = m_SessionObj.Get(helper.Service_CredSSP_Uri, 0);
                node = helper.GetXmlNode(result, helper.CredSSP_SNode, helper.Service_CredSSP_XMLNmsp);
                if (node.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
                    WriteObject(helper.GetResourceMsgFromResourcetext("CredSSPServiceConfigured"));
                    WriteObject(helper.GetResourceMsgFromResourcetext("CredSSPServiceNotConfigured"));
                ErrorRecord er = new ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, null);
                ErrorRecord er = new ErrorRecord(ex, "InvalidArgument", ErrorCategory.InvalidOperation, null);
                ErrorRecord er = new ErrorRecord(ex, "XPathException", ErrorCategory.InvalidOperation, null);

    #region Get-HotFix
    /// Cmdlet for Get-Hotfix Proxy.
    [Cmdlet(VerbsCommon.Get, "HotFix", DefaultParameterSetName = "Default",
        HelpUri = "https://go.microsoft.com/fwlink/?linkid=2109716", RemotingCapability = RemotingCapability.SupportedByCommand)]
    [OutputType(@"System.Management.ManagementObject#root\cimv2\Win32_QuickFixEngineering")]
    public sealed class GetHotFixCommand : PSCmdlet, IDisposable
        /// Specifies the HotFixID. Unique identifier associated with a particular update.
        [Parameter(Position = 0, ParameterSetName = "Default")]
        [Alias("HFID")]
        public string[] Id { get; set; }
        /// To search on description of Hotfixes.
        [Parameter(ParameterSetName = "Description")]
        public string[] Description { get; set; }
        /// Parameter to pass the Computer Name.
        [Alias("CN", "__Server", "IPAddress")]
        public string[] ComputerName { get; set; } = new string[] { "localhost" };
        /// Parameter to pass the Credentials.
        private ManagementObjectSearcher _searchProcess;
        private bool _inputContainsWildcard = false;
        private readonly ConnectionOptions _connectionOptions = new();
        /// Sets connection options.
            _connectionOptions.Authentication = AuthenticationLevel.Packet;
            _connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
            _connectionOptions.Username = Credential?.UserName;
            _connectionOptions.SecurePassword = Credential?.Password;
        /// Get the List of HotFixes installed on the Local Machine.
                bool foundRecord = false;
                StringBuilder queryString = new();
                ManagementScope scope = new(ComputerWMIHelper.GetScopeString(computer, ComputerWMIHelper.WMI_Path_CIM), _connectionOptions);
                if (Id != null)
                    queryString.Append("Select * from Win32_QuickFixEngineering where (");
                    for (int i = 0; i <= Id.Length - 1; i++)
                        queryString.Append("HotFixID= '");
                        queryString.Append(Id[i].Replace("'", "\\'"));
                        queryString.Append('\'');
                        if (i < Id.Length - 1)
                            queryString.Append(" Or ");
                    queryString.Append(')');
                    queryString.Append("Select * from Win32_QuickFixEngineering");
                    foundRecord = true;
                _searchProcess = new ManagementObjectSearcher(scope, new ObjectQuery(queryString.ToString()));
                foreach (ManagementObject obj in _searchProcess.Get())
                    if (Description != null)
                        if (!FilterMatch(obj))
                        _inputContainsWildcard = true;
                    // try to translate the SID to a more friendly username
                    // just stick with the SID if anything goes wrong
                    string installed = (string)obj["InstalledBy"];
                    if (!string.IsNullOrEmpty(installed))
                            SecurityIdentifier secObj = new(installed);
                            obj["InstalledBy"] = secObj.Translate(typeof(NTAccount));
                        catch (IdentityNotMappedException)
                            // thrown by SecurityIdentifier.Translate
                        catch (SystemException)
                            // thrown by SecurityIdentifier.constr
                if (!foundRecord && !_inputContainsWildcard)
                    Exception ex = new ArgumentException(StringUtil.Format(HotFixResources.NoEntriesFound, computer));
                    WriteError(new ErrorRecord(ex, "GetHotFixNoEntriesFound", ErrorCategory.ObjectNotFound, null));
                if (_searchProcess != null)
            _searchProcess?.Dispose();
        private bool FilterMatch(ManagementObject obj)
                foreach (string desc in Description)
                    WildcardPattern wildcardpattern = WildcardPattern.Get(desc, WildcardOptions.IgnoreCase);
                    if (wildcardpattern.IsMatch((string)obj["Description"]))
                    if (WildcardPattern.ContainsWildcardCharacters(desc))
        /// Release all resources.

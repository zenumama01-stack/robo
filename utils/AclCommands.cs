    /// Defines the base class from which all Security Descriptor commands
    /// are derived.
    public abstract class SecurityDescriptorCommandsBase : PSCmdlet
        /// Gets or sets the filter property.  The filter
        /// property allows for provider-specific filtering of results.
                return _filter;
                _filter = value;
        /// Gets or sets the include property.  The include property
        /// specifies the items on which the command will act.
        /// Gets or sets the exclude property.  The exclude property
        /// specifies the items on which the command will not act.
        internal CmdletProviderContext CmdletProviderContext
                coreCommandContext.SetFilters(includeFilter,
                                              excludeFilter,
                                              Filter);
        #region brokered properties
        /// Add brokered properties for easy access to important properties
        /// of security descriptor.
        internal static void AddBrokeredProperties(
            Collection<PSObject> results,
            bool audit,
            bool allCentralAccessPolicies)
            foreach (PSObject result in results)
                if (audit)
                    // Audit
                    result.Properties.Add
                        new PSCodeProperty
                                "Audit",
                                typeof(SecurityDescriptorCommandsBase).GetMethod("GetAudit")
                // CentralAccessPolicyId retrieval does not require elevation, so we always add this property.
                            "CentralAccessPolicyId",
                            typeof(SecurityDescriptorCommandsBase).GetMethod("GetCentralAccessPolicyId")
#if !CORECLR    // GetAllCentralAccessPolicies and GetCentralAccessPolicyName are not supported in OneCore powershell
                // because function 'LsaQueryCAPs' is not available in OneCoreUAP and NanoServer.
                if (allCentralAccessPolicies)
                    // AllCentralAccessPolicies
                                "AllCentralAccessPolicies",
                                typeof(SecurityDescriptorCommandsBase).GetMethod("GetAllCentralAccessPolicies")
                // CentralAccessPolicyName retrieval does not require elevation, so we always add this property.
                            "CentralAccessPolicyName",
                            typeof(SecurityDescriptorCommandsBase).GetMethod("GetCentralAccessPolicyName")
        /// Gets the Path of the provided PSObject.
        /// <param name="instance">
        /// The PSObject for which to obtain the path.
        /// The path of the provided PSObject.
        public static string GetPath(PSObject instance)
            if (instance == null)
                throw PSTraceSource.NewArgumentNullException(nameof(instance));
                // These are guaranteed to not be null, but even checking
                // them for null causes a presharp warning
                // Get path
                return instance.Properties["PSPath"].Value.ToString();
        /// Gets the Owner of the provided PSObject.
        /// The PSObject for which to obtain the Owner.
        /// The Owner of the provided PSObject.
        public static string GetOwner(PSObject instance)
            if (instance.BaseObject is not ObjectSecurity sd)
            // Get owner
                IdentityReference ir = sd.GetOwner(typeof(NTAccount));
                return ir.ToString();
                // All Acl cmdlets returning SIDs will return a string
                // representation of the SID in all cases where the SID
                // cannot be mapped to a proper user or group name.
            // We are here since we cannot get IdentityReference from sd..
            // So return sddl..
            return sd.GetSecurityDescriptorSddlForm(AccessControlSections.Owner);
        /// Gets the Group of the provided PSObject.
        /// The PSObject for which to obtain the Group.
        /// The Group of the provided PSObject.
        public static string GetGroup(PSObject instance)
            // Get Group
                IdentityReference ir = sd.GetGroup(typeof(NTAccount));
            return sd.GetSecurityDescriptorSddlForm(AccessControlSections.Group);
        /// Gets the access rules of the provided PSObject.
        /// The PSObject for which to obtain the access rules.
        /// The access rules of the provided PSObject.
        public static AuthorizationRuleCollection GetAccess(PSObject instance)
            ObjectSecurity sd = instance.BaseObject as ObjectSecurity;
            if (sd == null)
                PSTraceSource.NewArgumentException(nameof(instance));
            // Get DACL
            if (sd is CommonObjectSecurity cos)
                return cos.GetAccessRules(true, true, typeof(NTAccount));
                DirectoryObjectSecurity dos = sd as DirectoryObjectSecurity;
                Dbg.Diagnostics.Assert(dos != null, "Acl should be of type CommonObjectSecurity or DirectoryObjectSecurity");
                return dos.GetAccessRules(true, true, typeof(NTAccount));
        /// Gets the audit rules of the provided PSObject.
        /// The PSObject for which to obtain the audit rules.
        /// The audit rules of the provided PSObject.
        public static AuthorizationRuleCollection GetAudit(PSObject instance)
                return cos.GetAuditRules(true, true, typeof(NTAccount));
                return dos.GetAuditRules(true, true, typeof(NTAccount));
        /// Gets the central access policy ID of the provided PSObject.
        /// The PSObject for which to obtain the central access policy ID.
        /// The central access policy ID of the provided PSObject.
        public static SecurityIdentifier GetCentralAccessPolicyId(PSObject instance)
            SessionState sessionState = new();
            string path = sessionState.Path.GetUnresolvedProviderPathFromPSPath(
                GetPath(instance));
            IntPtr pSd = IntPtr.Zero;
                // Get the file's SACL containing the CAPID ACE.
                uint rs = NativeMethods.GetNamedSecurityInfo(
                    NativeMethods.SeObjectType.SE_FILE_OBJECT,
                    NativeMethods.SecurityInformation.SCOPE_SECURITY_INFORMATION,
                    out IntPtr pOwner,
                    out IntPtr pGroup,
                    out IntPtr pSacl,
                    out pSd);
                if (rs != NativeMethods.ERROR_SUCCESS)
                    throw new Win32Exception((int)rs);
                if (pSacl == IntPtr.Zero)
                NativeMethods.ACL sacl = Marshal.PtrToStructure<NativeMethods.ACL>(pSacl);
                if (sacl.AceCount == 0)
                // Extract the first CAPID from the SACL that does not have INHERIT_ONLY_ACE flag set.
                IntPtr pAce = pSacl + Marshal.SizeOf(new NativeMethods.ACL());
                for (ushort aceIdx = 0; aceIdx < sacl.AceCount; aceIdx++)
                    NativeMethods.ACE_HEADER ace = Marshal.PtrToStructure<NativeMethods.ACE_HEADER>(pAce);
                    Dbg.Diagnostics.Assert(ace.AceType ==
                        NativeMethods.SYSTEM_SCOPED_POLICY_ID_ACE_TYPE,
                        "Unexpected ACE type: " + ace.AceType.ToString(CultureInfo.CurrentCulture));
                    if ((ace.AceFlags & NativeMethods.INHERIT_ONLY_ACE) == 0)
                    pAce += ace.AceSize;
                IntPtr pSid = pAce + Marshal.SizeOf(new NativeMethods.SYSTEM_AUDIT_ACE()) -
                    Marshal.SizeOf(new uint());
                bool ret = NativeMethods.IsValidSid(pSid);
                return new SecurityIdentifier(pSid);
                NativeMethods.LocalFree(pSd);
#if !CORECLR
        /// Gets the central access policy name of the provided PSObject.
        /// Function 'LsaQueryCAPs' is not available in OneCoreUAP and NanoServer.
        /// The PSObject for which to obtain the central access policy name.
        /// The central access policy name of the provided PSObject.
        public static string GetCentralAccessPolicyName(PSObject instance)
            SecurityIdentifier capId = GetCentralAccessPolicyId(instance);
            if (capId == null)
                return null; // file does not have the scope ace
            int capIdSize = capId.BinaryLength;
            byte[] capIdArray = new byte[capIdSize];
            capId.GetBinaryForm(capIdArray, 0);
            IntPtr caps = IntPtr.Zero;
            IntPtr pCapId = Marshal.AllocHGlobal(capIdSize);
                // Retrieve the CAP by CAPID.
                Marshal.Copy(capIdArray, 0, pCapId, capIdSize);
                IntPtr[] ppCapId = new IntPtr[1];
                ppCapId[0] = pCapId;
                uint rs = NativeMethods.LsaQueryCAPs(
                    ppCapId,
                    out caps,
                    out uint capCount);
                if (rs != NativeMethods.STATUS_SUCCESS)
                if (capCount == 0 || caps == IntPtr.Zero)
                // Get the CAP name.
                NativeMethods.CENTRAL_ACCESS_POLICY cap = Marshal.PtrToStructure<NativeMethods.CENTRAL_ACCESS_POLICY>(caps);
                // LSA_UNICODE_STRING is composed of WCHARs, but its length is given in bytes.
                return Marshal.PtrToStringUni(cap.Name.Buffer, cap.Name.Length / 2);
                Marshal.FreeHGlobal(pCapId);
                uint rs = NativeMethods.LsaFreeMemory(caps);
                Dbg.Diagnostics.Assert(rs == NativeMethods.STATUS_SUCCESS,
                    "LsaFreeMemory failed: " + rs.ToString(CultureInfo.CurrentCulture));
        /// Gets the names and IDs of all central access policies available on the machine.
        /// The PSObject argument is ignored.
        /// The names and IDs of all central access policies available on the machine.
        public static string[] GetAllCentralAccessPolicies(PSObject instance)
                // Retrieve all CAPs.
                Dbg.Diagnostics.Assert(capCount < 0xFFFF,
                    "Too many central access policies");
                // Add CAP names and IDs to a string array.
                string[] policies = new string[capCount];
                IntPtr capPtr = caps;
                for (uint capIdx = 0; capIdx < capCount; capIdx++)
                    // Retrieve CAP name.
                    Dbg.Diagnostics.Assert(capPtr != IntPtr.Zero,
                        "Invalid central access policies array");
                    NativeMethods.CENTRAL_ACCESS_POLICY cap = Marshal.PtrToStructure<NativeMethods.CENTRAL_ACCESS_POLICY>(capPtr);
                    policies[capIdx] = "\"" + Marshal.PtrToStringUni(
                        cap.Name.Buffer,
                        cap.Name.Length / 2) + "\"";
                    // Retrieve CAPID.
                    IntPtr pCapId = cap.CAPID;
                    Dbg.Diagnostics.Assert(pCapId != IntPtr.Zero,
                    bool ret = NativeMethods.IsValidSid(pCapId);
                    SecurityIdentifier sid = new SecurityIdentifier(pCapId);
                    policies[capIdx] += " (" + sid.ToString() + ")";
                    capPtr += Marshal.SizeOf(cap);
                return policies;
        /// Gets the security descriptor (in SDDL form) of the
        /// provided PSObject.  SDDL form is the Security Descriptor
        /// Definition Language.
        /// The PSObject for which to obtain the security descriptor.
        /// The security descriptor of the provided PSObject, in SDDL form.
        public static string GetSddl(PSObject instance)
            string sddl = sd.GetSecurityDescriptorSddlForm(AccessControlSections.All);
            return sddl;
        #endregion brokered properties
        /// The filter to be used to when globbing to get the item.
        private string _filter;
        /// The glob string used to determine which items are included.
        /// The glob string used to determine which items are excluded.
    /// Defines the implementation of the 'get-acl' cmdlet.
    /// This cmdlet gets the security descriptor of an item at the specified path.
    [Cmdlet(VerbsCommon.Get, "Acl", SupportsTransactions = true, DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096593")]
    public sealed class GetAclCommand : SecurityDescriptorCommandsBase
        /// Initializes a new instance of the GetAclCommand
        /// class.  Sets the default path to the current location.
        public GetAclCommand()
            // Default for path is the current location
            _path = new string[] { "." };
        /// Gets or sets the path of the item for which to obtain the
        /// security descriptor.  Default is the current location.
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPath")]
        /// InputObject Parameter
        /// Gets or sets the inputObject for which to obtain the security descriptor.
        [Parameter(Mandatory = true, ParameterSetName = "ByInputObject")]
        /// Gets or sets the literal path of the item for which to obtain the
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByLiteralPath")]
        /// Gets or sets the audit flag of the command.  This flag
        /// determines if audit rules should also be retrieved.
        public SwitchParameter Audit
                return _audit;
                _audit = value;
        private SwitchParameter _audit;
        /// Parameter '-AllCentralAccessPolicies' is not supported in OneCore powershell,
        /// because function 'LsaQueryCAPs' is not available in OneCoreUAP and NanoServer.
        private SwitchParameter AllCentralAccessPolicies
        /// Gets or sets the AllCentralAccessPolicies flag of the command. This flag
        /// determines whether the information about all central access policies
        /// available on the machine should be displayed.
        public SwitchParameter AllCentralAccessPolicies
                return allCentralAccessPolicies;
                allCentralAccessPolicies = value;
        private SwitchParameter allCentralAccessPolicies;
        /// Processes records from the input pipeline.
        /// For each input file, the command retrieves its
        /// corresponding security descriptor.
            AccessControlSections sections =
                AccessControlSections.Owner |
                AccessControlSections.Group |
                AccessControlSections.Access;
            if (_audit)
                sections |= AccessControlSections.Audit;
                PSMethodInfo methodInfo = _inputObject.Methods["GetSecurityDescriptor"];
                if (methodInfo != null)
                    object customDescriptor = null;
                        customDescriptor = PSObject.Base(methodInfo.Invoke());
                        if (customDescriptor is not FileSystemSecurity)
                            customDescriptor = new CommonSecurityDescriptor(false, false, customDescriptor.ToString());
                        // Calling user code, Catch-all OK
                        ErrorRecord er =
                        SecurityUtils.CreateNotSupportedErrorRecord(
                            UtilsStrings.MethodInvokeFail,
                            "GetAcl_OperationNotSupported"
                    WriteObject(customDescriptor, true);
                            UtilsStrings.GetMethodNotFound,
                foreach (string p in Path)
                    string currentPath = null;
                            pathsToProcess.Add(p);
                            Collection<PathInfo> resolvedPaths =
                                SessionState.Path.GetResolvedPSPathFromPSPath(p, CmdletProviderContext);
                            foreach (PathInfo pi in resolvedPaths)
                                pathsToProcess.Add(pi.Path);
                        foreach (string rp in pathsToProcess)
                            currentPath = rp;
                            CmdletProviderContext context = new(this.Context);
                            context.SuppressWildcardExpansion = true;
                            if (!InvokeProvider.Item.Exists(rp, false, _isLiteralPath))
                                    SecurityUtils.CreatePathNotFoundErrorRecord(
                                               rp,
                                               "GetAcl_PathNotFound"
                            InvokeProvider.SecurityDescriptor.Get(rp, sections, context);
                            Collection<PSObject> sd = context.GetAccumulatedObjects();
                            if (sd != null)
                                AddBrokeredProperties(
                                    sd,
                                    _audit,
                                    AllCentralAccessPolicies);
                                WriteObject(sd, true);
                                UtilsStrings.OperationNotSupportedOnPath,
                                "GetAcl_OperationNotSupported",
                                currentPath
                                p,
                                "GetAcl_PathNotFound_Exception"
    /// Defines the implementation of the 'set-acl' cmdlet.
    /// This cmdlet sets the security descriptor of an item at the specified path.
    [Cmdlet(VerbsCommon.Set, "Acl", SupportsShouldProcess = true, SupportsTransactions = true, DefaultParameterSetName = "ByPath",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096600")]
    public sealed class SetAclCommand : SecurityDescriptorCommandsBase
        /// Gets or sets the path of the item for which to set the
        /// security descriptor.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPath")]
        /// Gets or sets the inputObject for which to set the security descriptor.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByInputObject")]
        /// Gets or sets the literal path of the item for which to set the
        private object _securityDescriptor;
        /// Gets or sets the security descriptor object to be
        /// set on the target item(s).
        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByPath")]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByLiteralPath")]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ByInputObject")]
        public object AclObject
                return _securityDescriptor;
                _securityDescriptor = PSObject.Base(value);
        /// Parameter '-CentralAccessPolicy' is not supported in OneCore powershell,
        private string CentralAccessPolicy { get; }
        private string centralAccessPolicy;
        /// Gets or sets the central access policy to be
        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPath")]
        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByLiteralPath")]
        public string CentralAccessPolicy
                return centralAccessPolicy;
                centralAccessPolicy = value;
        private SwitchParameter _clearCentralAccessPolicy;
        /// Clears the central access policy applied on the target item(s).
        [Parameter(Mandatory = false, ParameterSetName = "ByPath")]
        [Parameter(Mandatory = false, ParameterSetName = "ByLiteralPath")]
        public SwitchParameter ClearCentralAccessPolicy
                return _clearCentralAccessPolicy;
                _clearCentralAccessPolicy = value;
        private SwitchParameter _passthru;
        /// Gets or sets the Passthru flag for the operation.
        /// If true, the security descriptor is also passed
        /// down the output pipeline.
        public SwitchParameter Passthru
                return _passthru;
                _passthru = value;
        /// Returns a newly allocated SACL with no ACEs in it.
        /// Free the returned SACL by calling Marshal.FreeHGlobal.
        private static IntPtr GetEmptySacl()
            IntPtr pSacl = IntPtr.Zero;
            bool ret = true;
                // Calculate the size of the empty SACL, align to DWORD.
                uint saclSize = (uint)(Marshal.SizeOf(new NativeMethods.ACL()) +
                    Marshal.SizeOf(new uint()) - 1) & 0xFFFFFFFC;
                Dbg.Diagnostics.Assert(saclSize < 0xFFFF,
                    "Acl size must be less than max SD size of 0xFFFF");
                // Allocate and initialize the SACL.
                pSacl = Marshal.AllocHGlobal((int)saclSize);
                ret = NativeMethods.InitializeAcl(
                    pSacl,
                    saclSize,
                    NativeMethods.ACL_REVISION);
                    Marshal.FreeHGlobal(pSacl);
                    pSacl = IntPtr.Zero;
            return pSacl;
        /// Returns a newly allocated SACL with the supplied CAPID in it.
        /// So the parameter "-CentralAccessPolicy" is not supported on OneCore powershell,
        /// and thus this method won't be hit in OneCore powershell.
        private IntPtr GetSaclWithCapId(string capStr)
            IntPtr pCapId = IntPtr.Zero, pSacl = IntPtr.Zero;
            bool ret = true, freeCapId = true;
            uint rs = NativeMethods.STATUS_SUCCESS;
                // Convert the supplied SID from string to binary form.
                ret = NativeMethods.ConvertStringSidToSid(capStr, out pCapId);
                    // We may have got a CAP friendly name instead of CAPID.
                    // Enumerate all CAPs on the system and try to find one with
                    // a matching friendly name.
                    // If we retrieve the CAPID from the LSA, the CAPID need not
                    // be deallocated separately (but with the entire buffer
                    // returned by LsaQueryCAPs).
                    freeCapId = false;
                    rs = NativeMethods.LsaQueryCAPs(
                        return IntPtr.Zero;
                    // Find the supplied string among available CAP names, use the corresponding CAPID.
                        string capName = Marshal.PtrToStringUni(
                            cap.Name.Length / 2);
                        if (capName.Equals(capStr, StringComparison.OrdinalIgnoreCase))
                            pCapId = cap.CAPID;
                if (pCapId == IntPtr.Zero)
                    Exception e = new ArgumentException(UtilsStrings.InvalidCentralAccessPolicyIdentifier);
                        "SetAcl_CentralAccessPolicy",
                        AclObject));
                ret = NativeMethods.IsValidSid(pCapId);
                uint sidSize = NativeMethods.GetLengthSid(pCapId);
                // Calculate the size of the SACL with one CAPID ACE, align to DWORD.
                    Marshal.SizeOf(new NativeMethods.SYSTEM_AUDIT_ACE()) +
                    sidSize - 1) & 0xFFFFFFFC;
                // Add CAPID to the SACL.
                rs = NativeMethods.AddScopedPolicyIDAce(
                    NativeMethods.ACL_REVISION,
                    NativeMethods.SUB_CONTAINERS_AND_OBJECTS_INHERIT,
                    pCapId);
                    if (rs == NativeMethods.STATUS_INVALID_PARAMETER)
                        throw new ArgumentException(UtilsStrings.InvalidCentralAccessPolicyIdentifier);
                if (!ret || rs != NativeMethods.STATUS_SUCCESS)
                rs = NativeMethods.LsaFreeMemory(caps);
                if (freeCapId)
                    NativeMethods.LocalFree(pCapId);
        /// Returns the current thread or process token with the specified privilege enabled
        /// and the previous state of this privilege. Free the returned token
        /// by calling NativeMethods.CloseHandle.
        private static IntPtr GetTokenWithEnabledPrivilege(
            string privilege,
            NativeMethods.TOKEN_PRIVILEGE previousState)
            IntPtr pToken = IntPtr.Zero;
                // First try to open the thread token for privilege adjustment.
                ret = NativeMethods.OpenThreadToken(
                    NativeMethods.GetCurrentThread(),
                    NativeMethods.TOKEN_QUERY | NativeMethods.TOKEN_ADJUST_PRIVILEGES,
                    true,
                    out pToken);
                    if (Marshal.GetLastWin32Error() == NativeMethods.ERROR_NO_TOKEN)
                        // Client is not impersonating. Open the process token.
                        ret = NativeMethods.OpenProcessToken(
                            NativeMethods.GetCurrentProcess(),
                // Get the LUID of the specified privilege.
                NativeMethods.LUID luid = new();
                ret = NativeMethods.LookupPrivilegeValue(
                    privilege,
                    ref luid);
                // Enable the privilege.
                NativeMethods.TOKEN_PRIVILEGE newState = new();
                newState.PrivilegeCount = 1;
                newState.Privilege.Attributes = NativeMethods.SE_PRIVILEGE_ENABLED;
                newState.Privilege.Luid = luid;
                uint previousSize = 0;
                ret = NativeMethods.AdjustTokenPrivileges(
                    pToken,
                    ref newState,
                    (uint)Marshal.SizeOf(previousState),
                    ref previousState,
                    ref previousSize);
                    NativeMethods.CloseHandle(pToken);
                    pToken = IntPtr.Zero;
            return pToken;
        /// For each input file, the command sets its
        /// security descriptor to the specified
        /// Access Control List (ACL).
            ObjectSecurity aclObjectSecurity = _securityDescriptor as ObjectSecurity;
                PSMethodInfo methodInfo = _inputObject.Methods["SetSecurityDescriptor"];
                    string sddl;
                    if (aclObjectSecurity != null)
                        sddl = aclObjectSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                    else if (_securityDescriptor is CommonSecurityDescriptor aclCommonSD)
                        sddl = aclCommonSD.GetSddlForm(AccessControlSections.All);
                        Exception e = new ArgumentException("AclObject");
                            "SetAcl_AclObject",
                        methodInfo.Invoke(sddl);
                            "SetAcl_OperationNotSupported"
                            UtilsStrings.SetMethodNotFound,
                if (Path == null)
                    Exception e = new ArgumentException("Path");
                        "SetAcl_Path",
                if (aclObjectSecurity == null)
                if (CentralAccessPolicy != null || ClearCentralAccessPolicy)
                    if (!DownLevelHelper.IsWin8AndAbove())
                        Exception e = new ParameterBindingException();
                            "SetAcl_OperationNotSupported",
                if (CentralAccessPolicy != null && ClearCentralAccessPolicy)
                    Exception e = new ArgumentException(UtilsStrings.InvalidCentralAccessPolicyParameters);
                    SecurityUtils.CreateInvalidArgumentErrorRecord(
                NativeMethods.TOKEN_PRIVILEGE previousState = new();
                    if (CentralAccessPolicy != null)
                        pSacl = GetSaclWithCapId(CentralAccessPolicy);
                            SystemException e = new(UtilsStrings.GetSaclWithCapIdFail);
                            WriteError(new ErrorRecord(e,
                    else if (ClearCentralAccessPolicy)
                        pSacl = GetEmptySacl();
                            SystemException e = new(UtilsStrings.GetEmptySaclFail);
                                                        "SetAcl_ClearCentralAccessPolicy",
                        Collection<PathInfo> pathsToProcess = new();
                        CmdletProviderContext context = this.CmdletProviderContext;
                        context.PassThru = Passthru;
                            string pathStr = SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                                p, out ProviderInfo Provider, out PSDriveInfo Drive);
                            pathsToProcess.Add(new PathInfo(Drive, Provider, pathStr, SessionState));
                            pathsToProcess = SessionState.Path.GetResolvedPSPathFromPSPath(p, CmdletProviderContext);
                        foreach (PathInfo pathInfo in pathsToProcess)
                            if (ShouldProcess(pathInfo.Path))
                                    InvokeProvider.SecurityDescriptor.Set(pathInfo.Path,
                                                                          aclObjectSecurity,
                                        if (!pathInfo.Provider.NameEquals(Context.ProviderNames.FileSystem))
                                        // Enable the security privilege required to set SCOPE_SECURITY_INFORMATION.
                                        IntPtr pToken = GetTokenWithEnabledPrivilege("SeSecurityPrivilege", previousState);
                                        if (pToken == IntPtr.Zero)
                                            SystemException e = new(UtilsStrings.GetTokenWithEnabledPrivilegeFail);
                                                                        "SetAcl_AdjustTokenPrivileges",
                                        // Set the file's CAPID.
                                        uint rs = NativeMethods.SetNamedSecurityInfo(
                                            pathInfo.ProviderPath,
                                            pSacl);
                                        // Restore privileges to the previous state.
                                        if (pToken != IntPtr.Zero)
                                            uint newSize = 0;
                                            NativeMethods.AdjustTokenPrivileges(
                                                (uint)Marshal.SizeOf(newState),
                                                ref newSize);
                                            Exception e = new Win32Exception(
                                                (int)rs,
                                                UtilsStrings.SetCentralAccessPolicyFail);
                                                                       "SetAcl_SetNamedSecurityInfo",
                                            pathInfo.Path
#endif // !UNIX

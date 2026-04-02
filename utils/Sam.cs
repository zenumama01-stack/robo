using System.Management.Automation.SecurityAccountsManager.Extensions;
using System.Management.Automation.SecurityAccountsManager.Native;
using System.Management.Automation.SecurityAccountsManager.Native.NtSam;
[module: SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
namespace System.Management.Automation.SecurityAccountsManager
    /// Defines enumeration constants for enabling and disabling something.
    internal enum Enabling
        Disable = 0,
        Enable
    /// Managed version of the SAM_RID_ENUMERATION native structure,
    /// to be returned from the EnumerateLocalUsers method of Sam.
    /// Contains the original structure's members along with additional
    /// members of use.
    internal class SamRidEnumeration
#region Original struct members
        public UInt32 RelativeId;
#endregion Original struct members
#region Additional members
        public IntPtr domainHandle;     // The domain handle used to acquire the data.
#endregion Additional members
    /// Provides methods for manipulating local Users and Groups.
    internal class Sam : IDisposable
        private enum GroupProperties
            Name        = 0x0001,   // NOT changeable through Set-LocalGroup
            Description = 0x0002,
            AllSetable  = Description,
            AllReadable = AllSetable | Name
        /// Defines a set of flags, each corresponding to a member of LocalUser,
        /// which indicate fields to be updated.
        /// Although password can be set through Create-LocalUser and Set-LocalUser,
        /// it is not a member of LocalUser so does not appear in this enumeration.
        private enum UserProperties
            None                    = 0x0000,   // not actually a LocalUser member
            Name                    = 0x0001,   // NOT changeable through Set-LocalUser
            AccountExpires          = 0x0002,
            Description             = 0x0004,
            Enabled                 = 0x0008,   // NOT changeable through Set-LocalUser
            FullName                = 0x0010,
            PasswordChangeableDate  = 0x0020,
            PasswordExpires         = 0x0040,
            PasswordNeverExpires    = 0x0080,
            UserMayChangePassword   = 0x0100,
            PasswordRequired        = 0x0200,
            PasswordLastSet         = 0x0400,   // CANNOT be set by cmdlet
            LastLogon               = 0x0800,   // CANNOT be set by cmdlet
            // All properties that can be set through Set-LocalUser
            AllSetable              = AccountExpires
                                        | Description
                                        | FullName
                                        | PasswordChangeableDate
                                        | PasswordExpires
                                        | PasswordNeverExpires
                                        | UserMayChangePassword
                                        | PasswordRequired,
            // Properties that can be set by Create-LocalUser
            AllCreateable           = AllSetable | Name | Enabled,
            // Properties that can be read by e.g., Get-LocalUser
            AllReadable             = AllCreateable | PasswordLastSet | LastLogon
        private enum PasswordExpiredState
            Unchanged = -1,
            NotExpired = 0,
            Expired = 1
        internal enum ObjectAccess : uint
            AliasRead   = Win32.STANDARD_RIGHTS_READ
                            | ALIAS_LIST_MEMBERS,
            ALiasWrite  = Win32.STANDARD_RIGHTS_WRITE
                            | ALIAS_WRITE_ACCOUNT
                            | ALIAS_ADD_MEMBER
                            | ALIAS_REMOVE_MEMBER,
            UserAllAccess   = Win32.STANDARD_RIGHTS_REQUIRED
                                | USER_READ_PREFERENCES
                                | USER_READ_LOGON
                                | USER_LIST_GROUPS
                                | USER_READ_GROUP_INFORMATION
                                | USER_WRITE_PREFERENCES
                                | USER_CHANGE_PASSWORD
                                | USER_FORCE_PASSWORD_CHANGE
                                | USER_READ_GENERAL
                                | USER_READ_ACCOUNT
                                | USER_WRITE_ACCOUNT
                                | USER_WRITE_GROUP_INFORMATION,
            UserRead        = Win32.STANDARD_RIGHTS_READ
                                | USER_READ_GENERAL             // not in original USER_READ
                                | USER_READ_GROUP_INFORMATION,
            UserWrite       = Win32.STANDARD_RIGHTS_WRITE
        internal enum DomainAccess : uint
            AllAccess   = Win32.STANDARD_RIGHTS_REQUIRED
                            | DOMAIN_READ_OTHER_PARAMETERS
                            | DOMAIN_WRITE_OTHER_PARAMETERS
                            | DOMAIN_WRITE_PASSWORD_PARAMS
                            | DOMAIN_CREATE_USER
                            | DOMAIN_CREATE_GROUP
                            | DOMAIN_CREATE_ALIAS
                            | DOMAIN_GET_ALIAS_MEMBERSHIP
                            | DOMAIN_LIST_ACCOUNTS
                            | DOMAIN_READ_PASSWORD_PARAMETERS
                            | DOMAIN_LOOKUP
                            | DOMAIN_ADMINISTER_SERVER,
            Read        = Win32.STANDARD_RIGHTS_READ
                            | DOMAIN_READ_OTHER_PARAMETERS,
            Write       = Win32.STANDARD_RIGHTS_WRITE
            Max         = Win32.MAXIMUM_ALLOWED
        /// The operation under way. Used in the <see cref="Context"/> class.
        private enum ContextOperation
            New = 1,
            Enable,
            Disable,
            Remove,
            Rename,
            Set,
            AddMember,
            GetMember,
            RemoveMember
        /// The type of object currently operating with.
        /// used in the <see cref="Context"/> class.
        private enum ContextObjectType
            User = 1,
            Group
#region Internal Classes
        /// Holds information about the underway operation.
        /// Used primarily by the private ThrowOnFailure method when building
        /// Exception objects to throw.
        private sealed class Context
            public ContextOperation operation;
            public ContextObjectType type;
            public object target;
            public string objectId;
            public string memberId;
            /// Initialize a new Context object.
            /// <param name="operation">
            /// One of the <see cref="ContextOperation"/> enumerations indicating
            /// the type of operation under way.
            /// <param name="objectType">
            /// One of the <see cref="ContextObjectType"/> enumerations indicating
            /// the type of object (user or group) being used.
            /// <param name="objectIdentifier">
            /// A string containing the name of the object. This may be either a
            /// user/group name or a string representation of a SID.
            /// <param name="target">
            /// The target being operated on.
            /// <param name="memberIdentifier">
            /// A string containing the name of the member being added or removed
            /// from a group. Used only in such cases.
            public Context(ContextOperation operation,
                           ContextObjectType objectType,
                           string objectIdentifier,
                           string memberIdentifier = null)
                this.type = objectType;
                this.objectId = objectIdentifier;
                this.target = target;
                this.memberId = memberIdentifier;
            public Context()
            /// Gets a string containing the type of operation under way.
            public string OperationName
                get { return operation.ToString(); }
            /// Gets a string containing the type of object ("User" or "Group")
            /// being used.
            public string TypeNamne
                get { return type.ToString(); }
            /// Gets a string containing the name of the object being used.
            public string ObjectName
                get { return objectId; }
            /// Gets a string containing the name of the member being added to
            /// or removed from a group. Returns null if the operation does not
            /// involve group members.
                get { return memberId; }
        /// Contains basic information about an Account.
        /// AccountInfo is the return type from the private
        /// LookupAccountInfo method.
        private sealed class AccountInfo
            public string AccountName;
            public string DomainName;
            public SecurityIdentifier Sid;
            public Native.SID_NAME_USE Use;
                if (!string.IsNullOrEmpty(DomainName))
                    return DomainName + '\\' + AccountName;
                    return AccountName;
#endregion Internal Classes
        // Access rights
        private const UInt32 ALIAS_ADD_MEMBER       = 0x0001;
        private const UInt32 ALIAS_REMOVE_MEMBER    = 0x0002;
        private const UInt32 ALIAS_LIST_MEMBERS     = 0x0004;
        private const UInt32 ALIAS_READ_INFORMATION = 0x0008;
        private const UInt32 ALIAS_WRITE_ACCOUNT    = 0x0010;
        private const UInt32 USER_READ_GENERAL              = 0x0001;
        private const UInt32 USER_READ_PREFERENCES          = 0x0002;
        private const UInt32 USER_WRITE_PREFERENCES         = 0x0004;
        private const UInt32 USER_READ_LOGON                = 0x0008;
        private const UInt32 USER_READ_ACCOUNT              = 0x0010;
        private const UInt32 USER_WRITE_ACCOUNT             = 0x0020;
        private const UInt32 USER_CHANGE_PASSWORD           = 0x0040;
        private const UInt32 USER_FORCE_PASSWORD_CHANGE     = 0x0080;
        private const UInt32 USER_LIST_GROUPS               = 0x0100;
        private const UInt32 USER_READ_GROUP_INFORMATION    = 0x0200;
        private const UInt32 USER_WRITE_GROUP_INFORMATION   = 0x0400;
        private const UInt32 DOMAIN_READ_PASSWORD_PARAMETERS = 0x0001;
        private const UInt32 DOMAIN_WRITE_PASSWORD_PARAMS    = 0x0002;
        private const UInt32 DOMAIN_READ_OTHER_PARAMETERS    = 0x0004;
        private const UInt32 DOMAIN_WRITE_OTHER_PARAMETERS   = 0x0008;
        private const UInt32 DOMAIN_CREATE_USER              = 0x0010;
        private const UInt32 DOMAIN_CREATE_GROUP             = 0x0020;
        private const UInt32 DOMAIN_CREATE_ALIAS             = 0x0040;
        private const UInt32 DOMAIN_GET_ALIAS_MEMBERSHIP     = 0x0080;
        private const UInt32 DOMAIN_LIST_ACCOUNTS            = 0x0100;
        private const UInt32 DOMAIN_LOOKUP                   = 0x0200;
        private const UInt32 DOMAIN_ADMINISTER_SERVER        = 0x0400;
#region Static Data
        private static SecurityIdentifier worldSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
#endregion Static Data
        private IntPtr samHandle = IntPtr.Zero;
        private IntPtr localDomainHandle = IntPtr.Zero;
        private IntPtr builtinDomainHandle = IntPtr.Zero;
        private Context context = null;
        private string machineName = string.Empty;
        internal Sam()
            OpenHandles();
            // CoreCLR does not have Environment.MachineName,
            // so we'll use this instead.
            machineName = System.Net.Dns.GetHostName();
#region Public (Internal) Methods
        public string StripMachineName(string name)
            var mn = machineName + '\\';
            if (name.StartsWith(mn, StringComparison.CurrentCultureIgnoreCase))
                return name.Substring(mn.Length);
#region Local Groups
        /// Retrieve a named local group.
        /// <param name="groupName">Name of the desired local group.</param>
        /// A <see cref="LocalGroup"/> object containing information about
        /// the local group.
        /// <exception cref="GroupNotFoundException">
        /// Thrown when the named group cannot be found.
        internal LocalGroup GetLocalGroup(string groupName)
            context = new Context(ContextOperation.Get, ContextObjectType.Group, groupName, groupName);
            foreach (var sre in EnumerateGroups())
                if (sre.Name.Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                    return MakeLocalGroupObject(sre);   // return a populated group
            throw new GroupNotFoundException(groupName, context.target);
        /// Retrieve a local group by SID.
        /// <param name="sid">
        /// A <see cref="SecurityIdentifier"/> object identifying the desired group.
        /// Thrown when the specified group cannot be found.
        internal LocalGroup GetLocalGroup(SecurityIdentifier sid)
            context = new Context(ContextOperation.Get, ContextObjectType.Group, sid.ToString(), sid);
                if (RidToSid(sre.domainHandle, sre.RelativeId) == sid)
            throw new GroupNotFoundException(sid.ToString(), context.target);
        /// Create a local group.
        /// <param name="group">A <see cref="LocalGroup"/> object containing
        /// information about the local group to be created.
        /// A new LocalGroup object containing information about the newly
        /// created local group.
        /// <exception cref="GroupExistsException">
        /// Thrown when an attempt is made to create a local group that already
        /// exists.
        internal LocalGroup CreateLocalGroup(LocalGroup group)
            context = new Context(ContextOperation.New, ContextObjectType.Group, group.Name, group.Name);
            return CreateGroup(group, localDomainHandle);
        /// Update a local group with new property values.
        /// <param name="group">
        /// A <see cref="LocalGroup"/> object representing the group to be updated.
        /// <param name="changed">
        /// A LocalGroup object containing the desired changes.
        /// Currently, a group's description is the only changeable property.
        internal void UpdateLocalGroup(LocalGroup group, LocalGroup changed)
            context = new Context(ContextOperation.Set, ContextObjectType.Group, group.Name, group);
            UpdateGroup(group, changed);
        /// Remove a local group.
        /// A <see cref="SecurityIdentifier"/> object identifying the
        /// local group to be removed.
        internal void RemoveLocalGroup(SecurityIdentifier sid)
            context = new Context(ContextOperation.Remove, ContextObjectType.Group, sid.ToString(), sid);
            RemoveGroup(sid);
        /// A <see cref="LocalGroup"/> object containing
        /// information about the local group to be removed.
        internal void RemoveLocalGroup(LocalGroup group)
            context = new Context(ContextOperation.Remove, ContextObjectType.Group, group.Name, group);
            if (group.SID == null)
                context.target = group = GetLocalGroup(group.Name);
            RemoveGroup(group.SID);
        /// Rename a local group.
        /// A <see cref="SecurityIdentifier"/> object identifying
        /// the local group to be renamed.
        /// <param name="newName">
        /// A string containing the new name for the local group.
        internal void RenameLocalGroup(SecurityIdentifier sid, string newName)
            context = new Context(ContextOperation.Rename, ContextObjectType.Group, sid.ToString(), sid);
            RenameGroup(sid, newName);
        /// information about the local group to be renamed.
        internal void RenameLocalGroup(LocalGroup group, string newName)
            context = new Context(ContextOperation.Rename, ContextObjectType.Group, group.Name, group);
            RenameGroup(group.SID, newName);
        /// Get all local groups whose names satisfy the specified predicate.
        /// <param name="pred">
        /// Predicate that determines whether a group satisfies the conditions.
        /// An <see cref="IEnumerable{LocalGroup}"/> object containing LocalGroup
        /// objects that satisfy the predicate condition.
        internal IEnumerable<LocalGroup> GetMatchingLocalGroups(Predicate<string> pred)
            context = new Context(ContextOperation.Get, ContextObjectType.Group, string.Empty, null);
                if (pred(sre.Name))
                    context.target = sre.Name;
                    yield return MakeLocalGroupObject(sre);
        /// Get all local groups.
        /// An <see cref="IEnumerable{LocalGroup}"/> object containing a
        /// LocalGroup object for each local group.
        internal IEnumerable<LocalGroup> GetAllLocalGroups()
        /// Add members to a local group.
        /// A <see cref="LocalGroup"/> object identifying the group to
        /// which to add members.
        /// <param name="member">
        /// An object of type <see cref="LocalPrincipal"/> identifying
        /// the member to be added.
        /// An Exception object indicating any errors encountered.
        /// Thrown if the group could not be found.
        internal Exception AddLocalGroupMember(LocalGroup group, LocalPrincipal member)
            context = new Context(ContextOperation.AddMember, ContextObjectType.Group, group.Name, group);
            return AddGroupMember(group.SID, member);
        /// <param name="groupSid">
        /// A <see cref="SecurityIdentifier"/> object identifying the group to
        internal Exception AddLocalGroupMember(SecurityIdentifier groupSid, LocalPrincipal member)
            context = new Context(ContextOperation.AddMember, ContextObjectType.Group, groupSid.ToString(), groupSid);
            return AddGroupMember(groupSid, member);
        /// Retrieve members of a Local group.
        /// A <see cref="LocalGroup"/> object identifying the group whose members
        /// are requested.
        /// An IEnumerable of <see cref="LocalPrincipal"/> objects containing the group's
        /// members.
        internal IEnumerable<LocalPrincipal> GetLocalGroupMembers(LocalGroup group)
            context = new Context(ContextOperation.GetMember, ContextObjectType.Group, group.Name, group);
            return GetGroupMembers(group.SID);
        /// A <see cref="SecurityIdentifier"/> object identifying the group whose members
        internal IEnumerable<LocalPrincipal> GetLocalGroupMembers(SecurityIdentifier groupSid)
            context = new Context(ContextOperation.GetMember, ContextObjectType.Group, groupSid.ToString(), groupSid);
            return GetGroupMembers(groupSid);
        /// Remove members from a local group.
        /// A <see cref="LocalGroup"/> object identifying the group from
        /// which to remove members
        /// the member to be removed.
        internal Exception RemoveLocalGroupMember(LocalGroup group, LocalPrincipal member)
            context = new Context(ContextOperation.RemoveMember, ContextObjectType.Group, group.Name, group);
            return RemoveGroupMember(group.SID, member);
        /// A <see cref="SecurityIdentifier"/> object identifying the group from
        /// An Object of type <see cref="LocalPrincipal"/> identifying
        internal Exception RemoveLocalGroupMember(SecurityIdentifier groupSid, LocalPrincipal member)
            context = new Context(ContextOperation.RemoveMember, ContextObjectType.Group, groupSid.ToString(), groupSid);
            return RemoveGroupMember(groupSid, member);
#endregion Local Groups
#region Local Users
        /// Retrieve a named local user.
        /// <param name="userName">Name of the desired local user.</param>
        /// A <see cref="LocalUser"/> object containing information about
        /// the local user.
        /// <exception cref="UserNotFoundException">
        /// Thrown when the named user cannot be found.
        internal LocalUser GetLocalUser(string userName)
            context = new Context(ContextOperation.Get, ContextObjectType.User, userName, userName);
            foreach (var sre in EnumerateUsers())
                if (sre.Name.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
                    return MakeLocalUserObject(sre);
            throw new UserNotFoundException(userName, userName);
        /// Retrieve a local user by SID.
        /// A <see cref="SecurityIdentifier"/> object identifying the desired user.
        /// Thrown when the specified user cannot be found.
        internal LocalUser GetLocalUser(SecurityIdentifier sid)
            context = new Context(ContextOperation.Get, ContextObjectType.User, sid.ToString(), sid);
                    return MakeLocalUserObject(sre);    // return a populated user
            throw new UserNotFoundException(sid.ToString(), sid);
        /// Create a local user.
        /// <param name="user">A <see cref="LocalUser"/> object containing
        /// information about the local user to be created.
        /// <param name="password">A <see cref="System.Security.SecureString"/> containing
        /// the initial password to be set for the new local user. If this parameter is null,
        /// no password is set.
        /// <param name="setPasswordNeverExpires">
        /// Indicates whether PasswordNeverExpires was specified
        /// created local user.
        /// <exception cref="UserExistsException">
        /// Thrown when an attempt is made to create a local user that already
        internal LocalUser CreateLocalUser(LocalUser user, System.Security.SecureString password, bool setPasswordNeverExpires)
            context = new Context(ContextOperation.New, ContextObjectType.User, user.Name, user);
            return CreateUser(user, password, localDomainHandle, setPasswordNeverExpires);
        /// Remove a local user.
        /// the local user to be removed.
        internal void RemoveLocalUser(SecurityIdentifier sid)
            context = new Context(ContextOperation.Remove, ContextObjectType.User, sid.ToString(), sid);
            RemoveUser(sid);
        /// <param name="user">
        /// A <see cref="LocalUser"/> object containing
        /// information about the local user to be removed.
        internal void RemoveLocalUser(LocalUser user)
            context = new Context(ContextOperation.Remove, ContextObjectType.User, user.Name, user);
            if (user.SID == null)
                context.target = user = GetLocalUser(user.Name);
            RemoveUser(user.SID);
        /// Rename a local user.
        /// A <see cref="SecurityIdentifier"/> objects identifying
        /// the local user to be renamed.
        /// A string containing the new name for the local user.
        internal void RenameLocalUser(SecurityIdentifier sid, string newName)
            context = new Context(ContextOperation.Rename, ContextObjectType.User, sid.ToString(), sid);
            RenameUser(sid, newName);
        /// A <see cref="LocalUser"/> objects containing
        /// information about the local user to be renamed.
        internal void RenameLocalUser(LocalUser user, string newName)
            context = new Context(ContextOperation.Rename, ContextObjectType.User, user.Name, user);
            RenameUser(user.SID, newName);
        /// Enable or disable a Local User.
        /// A <see cref="SecurityIdentifier"/> object identifying the user to enable or disable.
        /// <param name="enable">
        /// One of the <see cref="Enabling"/> enumeration values, indicating whether to
        /// enable or disable the user.
        internal void EnableLocalUser(SecurityIdentifier sid, Enabling enable)
            context = new Context(enable == Enabling.Enable ? ContextOperation.Enable
                                                            : ContextOperation.Disable,
                                  ContextObjectType.User, sid.ToString(),
                                  sid);
            EnableUser(sid, enable);
        /// A <see cref="LocalUser"/> object representing the user to enable or disable.
        internal void EnableLocalUser(LocalUser user, Enabling enable)
                                  ContextObjectType.User, user.Name,
                                  user);
            EnableUser(user.SID, enable);
        /// Update a local user with new properties.
        /// A <see cref="LocalUser"/> object representing the user to be updated.
        /// A LocalUser object containing the desired changes.
        /// <param name="password">A <see cref="System.Security.SecureString"/>
        /// object containing the new password. A null value in this parameter
        /// indicates that the password is not to be changed.
        /// Specifies whether the PasswordNeverExpires parameter was set.
        /// Call this overload when intending to leave the password-expired
        /// marker in its current state. To set the password and the
        /// password-expired state, call the overload with a boolean as the
        /// fourth parameter
        internal void UpdateLocalUser(LocalUser user, LocalUser changed, System.Security.SecureString password, bool? setPasswordNeverExpires)
            context = new Context(ContextOperation.Set, ContextObjectType.User, user.Name, user);
            UpdateUser(user, changed, password, PasswordExpiredState.Unchanged, setPasswordNeverExpires);
        /// Get all local users whose names satisfy the specified predicate.
        /// Predicate that determines whether a user satisfies the conditions.
        /// An <see cref="IEnumerable{LocalUser}"/> object containing LocalUser
        internal IEnumerable<LocalUser> GetMatchingLocalUsers(Predicate<string> pred)
            context = new Context(ContextOperation.Get, ContextObjectType.User, string.Empty, null);
                    yield return MakeLocalUserObject(sre);
        /// Get all local users.
        /// An <see cref="IEnumerable{LocalUser}"/> object containing a
        /// LocalUser object for each local user.
        internal IEnumerable<LocalUser> GetAllLocalUsers()
            context = new Context(ContextOperation.Get, ContextObjectType.User, null, null);
#endregion Local Users
#region Local Principals
        internal LocalPrincipal LookupAccount(string name)
            var info = LookupAccountInfo(name);
            if (info == null)
                throw new PrincipalNotFoundException(name, name);
            return MakeLocalPrincipalObject(info);
#endregion Local Principals
#endregion Public (Internal) Methods
        /// Open the handles stored by Sam instances.
        private void OpenHandles()
            var systemName = new UNICODE_STRING();
            var oa = new OBJECT_ATTRIBUTES();
            IntPtr pInfo = IntPtr.Zero;
            IntPtr pSid = IntPtr.Zero;
            IntPtr lsaHandle = IntPtr.Zero;
            UInt32 status = 0;
                status = Win32.LsaOpenPolicy(ref systemName, ref oa, (UInt32)LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION, out lsaHandle);
                ThrowOnFailure(status);
                POLICY_PRIMARY_DOMAIN_INFO domainInfo;
                status = Win32.LsaQueryInformationPolicy(lsaHandle,
                                                         POLICY_INFORMATION_CLASS.PolicyAccountDomainInformation,
                                                         out pInfo);
                status = Win32.LsaClose(lsaHandle);
                lsaHandle = IntPtr.Zero;
                domainInfo = Marshal.PtrToStructure<POLICY_PRIMARY_DOMAIN_INFO>(pInfo);
                status = SamApi.SamConnect(ref systemName, out samHandle, SamApi.SAM_SERVER_LOOKUP_DOMAIN, ref oa);
                // Open the local domain
                status = SamApi.SamOpenDomain(samHandle, Win32.MAXIMUM_ALLOWED, domainInfo.Sid, out localDomainHandle);
                // Open the "BuiltIn" domain
                SecurityIdentifier sid = new SecurityIdentifier("S-1-5-32");
                byte[] bSid = new byte[sid.BinaryLength];
                int size = Marshal.SizeOf<byte>() * bSid.Length;
                pSid = Marshal.AllocHGlobal(size);
                sid.GetBinaryForm(bSid, 0);
                Marshal.Copy(bSid, 0, pSid, bSid.Length);
                status = SamApi.SamOpenDomain(samHandle, Win32.MAXIMUM_ALLOWED, pSid, out builtinDomainHandle);
                if (pInfo != IntPtr.Zero)
                    status = Win32.LsaFreeMemory(pInfo);
                Marshal.FreeHGlobal(pSid);
                if (lsaHandle != IntPtr.Zero)
        /// Find a group by SID and return a <see cref="SamRidEnumeration"/> object
        /// representing the group.
        /// <param name="sid">A <see cref="SecurityIdentifier"/> object identifying
        /// the group to search for.</param>
        /// A SamRidEnumeration object representing the group.
        /// Thrown when the specified group is not found.
        /// This method saves some time and effort over the GetGroup method
        /// because it does not have to open a group to populate a full Group
        /// object.
        private SamRidEnumeration GetGroupSre(SecurityIdentifier sid)
                    return sre;
            throw new GroupNotFoundException(sid.ToString(), sid);
        /// Find a user by SID and return a <see cref="SamRidEnumeration"/> object
        /// representing the user.
        /// the user to search for.</param>
        /// A SamRidEnumeration object representing the user.
        /// Thrown when the specified user is not found.
        /// This method saves some time and effort over the GetUser method
        /// because it does not have to open a user to populate a full LocalUser
        private SamRidEnumeration GetUserSre(SecurityIdentifier sid)
        /// Enumerate local users with native SAM functions.
        /// <param name="domainHandle">Handle to the domain to enumerate over.</param>
        /// An IEnumerable of SamRidEnumeration objects, one for each local user.
        /// This is a "generator" method. Rather than returning an entire collection,
        /// it uses 'yield return' to return each object in turn.
        private static IEnumerable<SamRidEnumeration> EnumerateUsersInDomain(IntPtr domainHandle)
            UInt32 context = 0;
            UInt32 countReturned;
                status = SamApi.SamEnumerateUsersInDomain(domainHandle,
                                                          ref context,
                                                          out buffer,
                                                          out countReturned);
                if (status == NtStatus.STATUS_MORE_ENTRIES && countReturned == 1)
                        SAM_RID_ENUMERATION sre;
                        sre = Marshal.PtrToStructure<SAM_RID_ENUMERATION>(buffer);
                        SamApi.SamFreeMemory(buffer);
                        buffer = IntPtr.Zero;
                        yield return new SamRidEnumeration
                                            Name = sre.Name.ToString(),
                                            RelativeId = sre.RelativeId,
                                            domainHandle = domainHandle
            } while (Succeeded(status) && status != 0 && countReturned != 0);
        /// Enumerate user objects in both the local and builtin domains.
        private IEnumerable<SamRidEnumeration> EnumerateUsers()
            foreach (var sre in EnumerateUsersInDomain(localDomainHandle))
                yield return sre;
            foreach (var sre in EnumerateUsersInDomain(builtinDomainHandle))
        /// Create a new user in the specified domain.
        /// <param name="userInfo">
        /// A <see cref="LocalUser"/> object containing information about the new user.
        /// <param name="domainHandle">
        /// Handle to the domain in which to create the new user.
        /// A LocalUser object that represents the newly-created user
        private LocalUser CreateUser(LocalUser userInfo, System.Security.SecureString password, IntPtr domainHandle, bool setPasswordNeverExpires)
            IntPtr userHandle = IntPtr.Zero;
            UNICODE_STRING str = new UNICODE_STRING();
                UInt32 relativeId = 0;
                UInt32 grantedAccess = 0;
                str = new UNICODE_STRING(userInfo.Name);
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(str));
                Marshal.StructureToPtr(str, buffer, false);
                status = SamApi.SamCreateUser2InDomain(domainHandle,
                    ref str,
                    (int) SamApi.USER_NORMAL_ACCOUNT,
                    Win32.MAXIMUM_ALLOWED,
                    out userHandle,
                    out grantedAccess,
                    out relativeId);
                Marshal.DestroyStructure<UNICODE_STRING>(buffer);
                // set the various properties of the user. A SID is required because some
                // operations depend on it.
                userInfo.SID = RidToSid(domainHandle, relativeId);
                SetUserData(userHandle, userInfo, UserProperties.AllCreateable, password, PasswordExpiredState.NotExpired, setPasswordNeverExpires);
                return MakeLocalUserObject(new SamRidEnumeration
                                                domainHandle = domainHandle,
                                                Name = userInfo.Name,
                                                RelativeId = relativeId
                                            userHandle);
                if (IntPtr.Zero != userHandle)
                    SamApi.SamDeleteUser(userHandle);
                if (userHandle != IntPtr.Zero)
                    status = SamApi.SamCloseHandle(userHandle);
        /// Remove a group identified by SID.
        /// group to be removed.
        private void RemoveGroup(SecurityIdentifier sid)
            var sre = GetGroupSre(sid);
            IntPtr aliasHandle = IntPtr.Zero;
            UInt32 status;
                status = SamApi.SamOpenAlias(sre.domainHandle,
                                                 sre.RelativeId,
                                                 out aliasHandle);
                status = SamApi.SamDeleteAlias(aliasHandle);
                aliasHandle = IntPtr.Zero; // The handle is freed internally if SamDeleteAlias succeeds
                if (aliasHandle != IntPtr.Zero)
                    status = SamApi.SamCloseHandle(aliasHandle);
        /// Rename a group identified by SID.
        /// A string containing the new name for the group.
        private void RenameGroup(SecurityIdentifier sid, string newName)
                ALIAS_NAME_INFORMATION info = new ALIAS_NAME_INFORMATION();
                info.Name = new UNICODE_STRING(newName);
                buffer = Marshal.AllocHGlobal(Marshal.SizeOf(info));
                Marshal.StructureToPtr(info, buffer, false);
                status = SamApi.SamSetInformationAlias(aliasHandle,
                                                       ALIAS_INFORMATION_CLASS.AliasNameInformation,
                ThrowOnFailure(status,
                               new Context {
                                            objectId = newName,
                                            operation = context.operation,
                                            type = context.type
                    Marshal.DestroyStructure<ALIAS_NAME_INFORMATION>(buffer);
        /// Add members to a group.
        /// An object of type <see cref="LocalPrincipal"/>identifying
        private Exception AddGroupMember(SecurityIdentifier groupSid, LocalPrincipal member)
            var sre = GetGroupSre(groupSid);    // We'll let this throw if necessary
            UInt32 status = SamApi.SamOpenAlias(sre.domainHandle,
            Exception ex = null;
                var sid = member.SID;
                var binarySid = new byte[sid.BinaryLength];
                sid.GetBinaryForm(binarySid, 0);
                status = SamApi.SamAddMemberToAlias(aliasHandle, binarySid);
                ex = MakeException(status,
                                            new Context
                                                memberId = member.ToString(),
                                                objectId = context.objectId,
                                                target = context.target,
            return ex;
        /// Retrieve members of a group.
        /// A <see cref="SecurityIdentifier"/> object representing the group whose members
        private IEnumerable<LocalPrincipal> GetGroupMembers(SecurityIdentifier groupSid)
            var sre = GetGroupSre(groupSid);
            IntPtr memberIds = IntPtr.Zero;
                UInt32 memberCount = 0;
                status = SamApi.SamGetMembersInAlias(aliasHandle, out memberIds, out memberCount);
                if (memberCount != 0)
                    IntPtr[] idArray = new IntPtr[memberCount];
                    Marshal.Copy(memberIds, idArray, 0, (int)memberCount);
                    for (int i=0; i < memberCount; i++)
                        var sid = new SecurityIdentifier(idArray[i]);
                        yield return MakeLocalPrincipalObject(LookupAccountInfo(sid));
                    SamApi.SamCloseHandle(aliasHandle);
                if (memberIds != IntPtr.Zero)
                    SamApi.SamFreeMemory(memberIds);
        /// Remove members from a group.
        /// An IEnumerable of Exception objects indicating any errors encountered.
        private Exception RemoveGroupMember(SecurityIdentifier groupSid, LocalPrincipal member)
            // Now we're processing each member, so any further exceptions will
            // be stored in the collection and returned later.
            var rv = new List<Exception>();
                status = SamApi.SamRemoveMemberFromAlias(aliasHandle, binarySid);
        /// Create a populated LocalUser object from a SamRidEnumeration object.
        /// <param name="sre">
        /// A <see cref="SamRidEnumeration"/> object containing minimal information
        /// about a local user.
        /// A LocalUser object, populated with user information.
        private LocalUser MakeLocalUserObject(SamRidEnumeration sre)
            var status = SamApi.SamOpenUser(sre.domainHandle,
                                            (UInt32)ObjectAccess.UserRead,
                                            out userHandle);
                return MakeLocalUserObject(sre, userHandle);
        /// Create a populated LocalUser object from a SamRidEnumeration object,
        /// using an already-opened SAM user handle.
        /// <param name="userHandle">
        /// Handle to an open SAM user.
        private LocalUser MakeLocalUserObject(SamRidEnumeration sre, IntPtr userHandle)
                USER_ALL_INFORMATION allInfo;
                status = SamApi.SamQueryInformationUser(userHandle,
                                                        USER_INFORMATION_CLASS.UserAllInformation,
                                                        out buffer);
                allInfo = Marshal.PtrToStructure<USER_ALL_INFORMATION>(buffer);
                var userSid = RidToSid(sre.domainHandle, sre.RelativeId);
                LocalUser user = new LocalUser()
                                        PrincipalSource = GetPrincipalSource(sre),
                                        SID = userSid,
                                        Name = allInfo.UserName.ToString(),
                                        FullName = allInfo.FullName.ToString(),
                                        Description = allInfo.AdminComment.ToString(),
                                        // TODO: why is this coming up as 864000000000 (number of ticks per day)?
                                        PasswordChangeableDate = DateTimeFromSam(allInfo.PasswordCanChange.QuadPart),
                                        PasswordExpires = DateTimeFromSam(allInfo.PasswordMustChange.QuadPart),
                                        // TODO: why is this coming up as 0X7FFFFFFFFFFFFFFF (largest signed 64-bit, and well out of range of DateTime)?
                                        AccountExpires = DateTimeFromSam(allInfo.AccountExpires.QuadPart),
                                        LastLogon = DateTimeFromSam(allInfo.LastLogon.QuadPart),
                                        PasswordLastSet = DateTimeFromSam(allInfo.PasswordLastSet.QuadPart),
                                        UserMayChangePassword = GetUserMayChangePassword(userHandle, userSid),
                                        PasswordRequired = (allInfo.UserAccountControl & SamApi.USER_PASSWORD_NOT_REQUIRED) == 0,
                                        Enabled = !((allInfo.UserAccountControl & SamApi.USER_ACCOUNT_DISABLED) == SamApi.USER_ACCOUNT_DISABLED)
                return user;
                    status = SamApi.SamFreeMemory(buffer);
        /// Enable or disable a user.
        /// A <see cref="SecurityIdentifier"/> object identifying the user to be
        /// enabled or disabled.
        /// One of the <see cref="Enabling"/> enumeration values indicating
        /// whether the user is to be enabled or disabled.
        private void EnableUser(SecurityIdentifier sid, Enabling enable)
            var sre = GetUserSre(sid);
            status = SamApi.SamOpenUser(sre.domainHandle,
                USER_ALL_INFORMATION info;
                info = Marshal.PtrToStructure<USER_ALL_INFORMATION>(buffer);
                UInt32 uac = info.UserAccountControl;
                UInt32 enabled_state = uac & SamApi.USER_ACCOUNT_DISABLED;
                if (enable == Enabling.Enable && enabled_state == SamApi.USER_ACCOUNT_DISABLED)
                    uac &= ~SamApi.USER_ACCOUNT_DISABLED;
                else if (enable == Enabling.Disable && enabled_state != SamApi.USER_ACCOUNT_DISABLED)
                    uac |= SamApi.USER_ACCOUNT_DISABLED;
                if (uac != info.UserAccountControl)
                    info.UserAccountControl = uac;
                    info.WhichFields = SamApi.USER_ALL_USERACCOUNTCONTROL;
                    status = SamApi.SamSetInformationUser(userHandle,
                    Marshal.DestroyStructure<USER_ALL_INFORMATION>(buffer);
        /// Rename a user.
        /// renamed.
        /// <param name="newName">The new user name.</param>
        private void RenameUser(SecurityIdentifier sid, string newName)
                USER_ACCOUNT_NAME_INFORMATION info = new USER_ACCOUNT_NAME_INFORMATION();
                info.UserName = new UNICODE_STRING(newName);
                                                      USER_INFORMATION_CLASS.UserAccountNameInformation,
                    Marshal.DestroyStructure<USER_ACCOUNT_NAME_INFORMATION>(buffer);
        /// Delete a user.
        /// removed.
        private void RemoveUser(SecurityIdentifier sid)
                status = SamApi.SamDeleteUser(userHandle);
                userHandle = IntPtr.Zero; // The handle is freed internally if SamDeleteUser succeeds
        private static IEnumerable<SamRidEnumeration> EnumerateGroupsInDomain(IntPtr domainHandle)
                // Although the method name indicates that we are operating with "groups",
                // it actually uses the SAM API's SamEnumerateAliasesInDomain function.
                status = SamApi.SamEnumerateAliasesInDomain(domainHandle,
        /// Enumerate group objects in both the local and builtin domains.
        /// An IEnumerable of SamRidEnumeration objects, one for each local group.
        internal IEnumerable<SamRidEnumeration> EnumerateGroups()
            foreach (var sre in EnumerateGroupsInDomain(localDomainHandle))
            foreach (var sre in EnumerateGroupsInDomain(builtinDomainHandle))
        /// Create a new group in the specified domain.
        /// <param name="groupInfo">
        /// A <see cref="LocalGroup"/> object containing information about the new group.
        /// <param name="domainHandle">Handle to the domain in which to create the new group.</param>
        /// A LocalGroup object that represents the newly-created group.
        private LocalGroup CreateGroup(LocalGroup groupInfo, IntPtr domainHandle)
                UInt32 relativeId;
                str = new UNICODE_STRING(groupInfo.Name);
                status = SamApi.SamCreateAliasInDomain(domainHandle,
                                                       out aliasHandle,
                if (!string.IsNullOrEmpty(groupInfo.Description))
                    ALIAS_ADM_COMMENT_INFORMATION info = new ALIAS_ADM_COMMENT_INFORMATION();
                    info.AdminComment = new UNICODE_STRING(groupInfo.Description);
                                                           ALIAS_INFORMATION_CLASS.AliasAdminCommentInformation,
                    Marshal.DestroyStructure<ALIAS_ADM_COMMENT_INFORMATION>(buffer);
                return MakeLocalGroupObject(new SamRidEnumeration
                                                    Name = groupInfo.Name,
                                            aliasHandle);
        /// Update a local group with new property values. This method provides
        /// the actual implementation.
        private void UpdateGroup(LocalGroup group, LocalGroup changed)
            // Only description may be changed
            if (group.Description == changed.Description)
                group = GetLocalGroup(group.Name);
            var sre = GetGroupSre(group.SID);
                info.AdminComment = new UNICODE_STRING(changed.Description);
        /// Create a populated LocalGroup object from a SamRidEnumeration object.
        /// about a local group.
        /// A LocalGroup object, populated with group information.
        private LocalGroup MakeLocalGroupObject(SamRidEnumeration sre)
            var status = SamApi.SamOpenAlias(sre.domainHandle,
                return MakeLocalGroupObject(sre, aliasHandle);
        /// Create a populated LocalGroup object from a SamRidEnumeration object,
        /// using an already-opened SAM alias handle.
        /// <param name="aliasHandle">
        /// Handle to an open SAM alias.
        private LocalGroup MakeLocalGroupObject(SamRidEnumeration sre, IntPtr aliasHandle)
                ALIAS_GENERAL_INFORMATION generalInfo;
                status = SamApi.SamQueryInformationAlias(aliasHandle,
                                                         ALIAS_INFORMATION_CLASS.AliasGeneralInformation,
                generalInfo = Marshal.PtrToStructure<ALIAS_GENERAL_INFORMATION>(buffer);
                LocalGroup group = new LocalGroup()
                                        SID = RidToSid(sre.domainHandle, sre.RelativeId),
                                        Name = generalInfo.Name.ToString(),
                                        Description = generalInfo.AdminComment.ToString()
                return group;
        /// <param name="passwordExpired">One of the
        /// <see cref="PasswordExpiredState"/> enumeration values indicating
        /// whether the password-expired state is to be explicitly set or
        /// left as is.
        /// If the <paramref name="password"/> parameter is null, this parameter
        /// is ignored.
        /// Indicates whether the PasswordNeverExpires parameter was specified.
        private void UpdateUser(LocalUser user,
                                LocalUser changed,
                                System.Security.SecureString password,
                                PasswordExpiredState passwordExpired,
                                bool? setPasswordNeverExpires)
            UserProperties properties = UserProperties.None;
            if (user.AccountExpires != changed.AccountExpires)
                properties |= UserProperties.AccountExpires;
            if (user.Description != changed.Description)
                properties |= UserProperties.Description;
            if (user.FullName != changed.FullName)
                properties |= UserProperties.FullName;
            if (setPasswordNeverExpires.HasValue)
                properties |= UserProperties.PasswordNeverExpires;
            if (user.UserMayChangePassword != changed.UserMayChangePassword)
                properties |= UserProperties.UserMayChangePassword;
            if (user.PasswordRequired != changed.PasswordRequired)
                properties |= UserProperties.PasswordRequired;
            if (   properties != UserProperties.None
                || passwordExpired != PasswordExpiredState.Unchanged
                || password != null)
                    status = SamApi.SamOpenUser(localDomainHandle,
                                                user.SID.GetRid(),
                    SetUserData(userHandle, changed, properties, password, passwordExpired, setPasswordNeverExpires);
        /// Set selected properties of a user.
        /// <param name="userHandle">Handle to an open SAM user.</param>
        /// <param name="sourceUser">
        /// A <see cref="LocalUser"/> object containing the data to set into the user.
        /// <param name="setFlags">
        /// A combination of <see cref="UserProperties"/> values indicating the properties to be set.
        /// object containing the new password.
        /// left as is. If the <paramref name="password"/> parameter is null,
        /// this parameter is ignored.
        /// Nullable value the specifies whether the PasswordNeverExpires bit should be flipped
        private void SetUserData(IntPtr userHandle,
                                 LocalUser sourceUser,
                                 UserProperties setFlags,
                UInt32 which = 0;
                UInt32 uac = GetUserAccountControl(userHandle);
                USER_ALL_INFORMATION info = new USER_ALL_INFORMATION();
                if (setFlags.HasFlag(UserProperties.AccountExpires))
                    which |= SamApi.USER_ALL_ACCOUNTEXPIRES;
                    info.AccountExpires.QuadPart = sourceUser.AccountExpires.HasValue
                                                 ? sourceUser.AccountExpires.Value.ToFileTime()
                                                 : 0L;
                if (setFlags.HasFlag(UserProperties.Description))
                    which |= SamApi.USER_ALL_ADMINCOMMENT;
                    info.AdminComment = new UNICODE_STRING(sourceUser.Description);
                if (setFlags.HasFlag(UserProperties.Enabled))
                    which |= SamApi.USER_ALL_USERACCOUNTCONTROL;
                    if (sourceUser.Enabled)
                if (setFlags.HasFlag(UserProperties.FullName))
                    which |= SamApi.USER_ALL_FULLNAME;
                    info.FullName = new UNICODE_STRING(sourceUser.FullName);
                if (setFlags.HasFlag(UserProperties.PasswordNeverExpires))
                    // Only modify the bit if a change was requested
                        if (setPasswordNeverExpires.Value)
                            uac |= SamApi.USER_DONT_EXPIRE_PASSWORD;
                            uac &= ~SamApi.USER_DONT_EXPIRE_PASSWORD;
                if (setFlags.HasFlag(UserProperties.PasswordRequired))
                    if (sourceUser.PasswordRequired)
                        uac &= ~SamApi.USER_PASSWORD_NOT_REQUIRED;
                        uac |= SamApi.USER_PASSWORD_NOT_REQUIRED;
                if (which != 0)
                    info.WhichFields = which;
                    if ((which & SamApi.USER_ALL_USERACCOUNTCONTROL) != 0)
                    buffer = Marshal.AllocHGlobal(Marshal.SizeOf<USER_ALL_INFORMATION>());
                    Marshal.StructureToPtr<USER_ALL_INFORMATION>(info, buffer, false);
                if (setFlags.HasFlag(UserProperties.UserMayChangePassword))
                    SetUserMayChangePassword(userHandle, sourceUser.SID, sourceUser.UserMayChangePassword);
                if (password != null)
                    SetUserPassword(userHandle, password, passwordExpired);
        /// Retrieve the User's User Account Control flags.
        /// Handle to an open user.
        /// A 32-bit unsigned integer containing the User Account Control
        /// flags as a set of bits.
        private UInt32 GetUserAccountControl(IntPtr userHandle)
            USER_LOGON_INFORMATION info;
                                                        USER_INFORMATION_CLASS.UserLogonInformation,
                info = Marshal.PtrToStructure<USER_LOGON_INFORMATION>(buffer);
                return info.UserAccountControl;
        /// Retrieve the DACL from a SAM object.
        /// <param name="objectHandle">
        /// A handle to the SAM object whose DACL is to be retrieved.
        /// A <see cref="RawAcl"/> object containing the DACL retrieved from
        /// the SAM object.
        private RawAcl GetSamDacl(IntPtr objectHandle)
            RawAcl rv = null;
            IntPtr securityObject = IntPtr.Zero;
                status = SamApi.SamQuerySecurityObject(objectHandle, Win32.DACL_SECURITY_INFORMATION, out securityObject);
                SECURITY_DESCRIPTOR sd = Marshal.PtrToStructure<SECURITY_DESCRIPTOR>(securityObject);
                bool daclPresent;
                bool daclDefaulted;
                IntPtr dacl;
                bool ok = Win32.GetSecurityDescriptorDacl(securityObject, out daclPresent, out dacl, out daclDefaulted);
                    var error = Marshal.GetLastWin32Error();
                    if (error == Win32.ERROR_ACCESS_DENIED)
                        throw new AccessDeniedException(context.target);
                        throw new Win32InternalException(error, context.target);
                if (daclPresent)
                    ACL acl = Marshal.PtrToStructure<ACL>(dacl);
                    if (acl.AclSize != 0)
                        // put the DACL into managed data
                        var bytes = new byte[acl.AclSize];
                        Marshal.Copy(dacl, bytes, 0, acl.AclSize);
                        rv = new RawAcl(bytes, 0);
                if (IntPtr.Zero != securityObject)
                    status = SamApi.SamFreeMemory(securityObject);
        /// Set the DACL of a SAM object.
        /// <param name="rawAcl">
        /// A <see cref="RawAcl"/> object containing the DACL to be set into
        private void SetSamDacl(IntPtr objectHandle, RawAcl rawAcl)
            IntPtr ipsd = IntPtr.Zero;
            IntPtr ipDacl = IntPtr.Zero;
                bool present = false;
                // create a new security descriptor
                var sd = new SECURITY_DESCRIPTOR() { Revision = 1 };
                ipsd = Marshal.AllocHGlobal(Marshal.SizeOf<SECURITY_DESCRIPTOR>());
                if (rawAcl != null && rawAcl.BinaryLength > 0)
                    Marshal.StructureToPtr<SECURITY_DESCRIPTOR>(sd, ipsd, false);
                    // put the DACL into unmanaged memory
                    var length = rawAcl.BinaryLength;
                    var bytes = new byte[length];
                    rawAcl.GetBinaryForm(bytes, 0);
                    ipDacl = Marshal.AllocHGlobal(length);
                    Marshal.Copy(bytes, 0, ipDacl, length);
                    present = true;
                // set the DACL into our new security descriptor
                var ok = Win32.SetSecurityDescriptorDacl(ipsd, present, ipDacl, false);
                var status = SamApi.SamSetSecurityObject(objectHandle, Win32.DACL_SECURITY_INFORMATION, ipsd);
                Marshal.FreeHGlobal(ipDacl);
                Marshal.FreeHGlobal(ipsd);
        /// Determine if a user account password may be changed by the user.
        /// Handle to a SAM user object.
        /// <param name="userSid">
        /// A <see cref="SecurityIdentifier"/> object identifying the SAM
        /// object's associated user.
        /// True if the user account password may be changed by its user,
        /// The ability to for the user to change the user account password
        /// is a permission in the object's DACL. This method walks through
        /// the ACEs in the DACL, checking if the permission is granted to
        /// either Everyone or the user identified by the userSid parameter.
        private bool GetUserMayChangePassword(IntPtr userHandle, SecurityIdentifier userSid)
            var rawAcl = GetSamDacl(userHandle);
            // if there is no DACL, then access is granted
            if (rawAcl == null)
            foreach (var a in rawAcl)
                var ace = a as CommonAce;
                if (ace != null && ace.AceType == AceType.AccessAllowed)
                    if (ace.SecurityIdentifier == worldSid ||
                        ace.SecurityIdentifier == userSid)
                        if ((ace.AccessMask & SamApi.USER_CHANGE_PASSWORD) != 0)
        /// Set whether a user account password may be changed by the user.
        /// A boolean indicating whether the permission is to be enabled or
        /// disabled.
        /// the ACEs in the DACL, enabling or disabling the permission on ACEs
        /// associated with either Everyone or the user identified by the
        /// userSid parameter.
        private void SetUserMayChangePassword(IntPtr userHandle, SecurityIdentifier userSid, bool enable)
            var changed = false;
            if (rawAcl != null)
                            if (enable)
                                ace.AccessMask |= SamApi.USER_CHANGE_PASSWORD;
                                ace.AccessMask &= ~SamApi.USER_CHANGE_PASSWORD;
                            changed = true;
                if (changed)
                    SetSamDacl(userHandle, rawAcl);
        /// Determine if a user's password has expired.
        /// Handle to an open User.
        /// True if the user's password has expired, false otherwise.
        private bool IsPasswordExpired(IntPtr userHandle)
                return info.PasswordExpired;
        /// Set a user's password.
        /// <param name="userHandle">Handle to an open User.</param>
        private void SetUserPassword(IntPtr userHandle,
                                     PasswordExpiredState passwordExpired)
                USER_SET_PASSWORD_INFORMATION info = new USER_SET_PASSWORD_INFORMATION();
                    bool setPwExpire = false;
                    switch (passwordExpired)
                        case PasswordExpiredState.Expired:
                            setPwExpire = true;
                        case PasswordExpiredState.NotExpired:
                            setPwExpire = false;
                        case PasswordExpiredState.Unchanged:
                            setPwExpire = IsPasswordExpired(userHandle);
                    info.Password = new UNICODE_STRING(password.AsString());
                    info.PasswordExpired = setPwExpire;
                    Marshal.StructureToPtr<USER_SET_PASSWORD_INFORMATION>(info, buffer, false);
                    var status = SamApi.SamSetInformationUser(userHandle,
                                                              USER_INFORMATION_CLASS.UserSetPasswordInformation,
                        Marshal.DestroyStructure<USER_SET_PASSWORD_INFORMATION>(buffer);
#region Utility Methods
        /// Create a <see cref="System.Security.Principal.SecurityIdentifier"/>
        /// object from a relative ID.
        /// Handle to the domain from which the ID was acquired.
        /// <param name="rid">
        /// The Relative ID value.
        /// A SecurityIdentifier object containing the SID of the
        /// object identified by the <paramref name="rid"/> parameter.
        private SecurityIdentifier RidToSid(IntPtr domainHandle, uint rid)
            IntPtr sidBytes = IntPtr.Zero;
                status = SamApi.SamRidToSid(domainHandle, rid, out sidBytes);
                if (status == NtStatus.STATUS_NOT_FOUND)
                    throw new InternalException(status,
                        StringUtil.Format(Strings.RidToSidFailed, rid),
                sid = new SecurityIdentifier(sidBytes);
                if (IntPtr.Zero != sidBytes)
                    status = SamApi.SamFreeMemory(sidBytes);
        /// Lookup the account identified by the specified SID.
        /// A <see cref="SecurityIdentifier"/> object identifying the account
        /// to look up.
        /// A <see cref="AccountInfo"/> object contains information about the
        /// account, or null if no matching account was found.
        private AccountInfo LookupAccountInfo(SecurityIdentifier sid)
            var sbAccountName = new StringBuilder();
            var sbDomainName = new StringBuilder();
            var accountNameLength = sbAccountName.Capacity;
            var domainNameLength = sbDomainName.Capacity;
            SID_NAME_USE use;
            var error = Win32.NO_ERROR;
            var bytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bytes, 0);
            if (!Win32.LookupAccountSid(null,
                                        bytes,
                                        sbAccountName, ref accountNameLength,
                                        sbDomainName, ref domainNameLength,
                                        out use))
                if (error == Win32.ERROR_INSUFFICIENT_BUFFER)
                    sbAccountName.EnsureCapacity(accountNameLength);
                    sbDomainName.EnsureCapacity((int)domainNameLength);
                    error = Win32.NO_ERROR;
            if (error == Win32.ERROR_SUCCESS)
                return new AccountInfo
                                AccountName = sbAccountName.ToString(),
                                DomainName = sbDomainName.ToString(),
                                Sid = sid,
                                Use = use
            else if (error == Win32.ERROR_NONE_MAPPED)
        /// Lookup the account identified by specified account name.
        /// <param name="accountName">
        /// A string containing the name of the account to look up.
        private AccountInfo LookupAccountInfo(string accountName)
            var domainNameLength = (uint)sbDomainName.Capacity;
            byte [] sid = null;
            uint sidLength = 0;
            int error = Win32.NO_ERROR;
            if (!Win32.LookupAccountName(null,
                                         accountName,
                                         sid,
                                         ref sidLength,
                                         sbDomainName,
                                         ref domainNameLength,
                if (error == Win32.ERROR_INSUFFICIENT_BUFFER || error == Win32.ERROR_INVALID_FLAGS)
                    sid = new byte[sidLength];
                // Bug: 7407413 :
                // If accountname is in the format domain1\user1,
                // then AccountName.ToString() will return domain1\domain1\user1
                // Ideally , accountname should be processed to hold only account name (without domain)
                // as we are keeping the domain in 'DomainName' variable.
                int index = accountName.IndexOf("\\", StringComparison.CurrentCultureIgnoreCase);
                if (index > -1)
                    accountName = accountName.Substring(index + 1);
                    AccountName = accountName,
                    Sid = new SecurityIdentifier(sid, 0),
            else if (error == Win32.ERROR_ACCESS_DENIED)
        /// Create a <see cref="LocalPrincipal"/> object from information in
        /// an AccountInfo object.
        /// An AccountInfo object containing information about the account
        /// for which the LocalPrincipal object is being created. This parameter
        /// may be null, in which case this method returns null.
        /// A new LocalPrincipal object representing the account, or null if the
        /// <paramref name="info"/> parameter is null.
        private LocalPrincipal MakeLocalPrincipalObject(AccountInfo info)
                return null;    // this is a legitimate case
            var rv = new LocalPrincipal(info.ToString());
            rv.SID = info.Sid;
            rv.PrincipalSource = GetPrincipalSource(info);
            switch (info.Use)
                case SID_NAME_USE.SidTypeAlias:     // TODO: is this the right thing to do???
                case SID_NAME_USE.SidTypeGroup:
                case SID_NAME_USE.SidTypeWellKnownGroup:
                    rv.ObjectClass = Strings.ObjectClassGroup;
                case SID_NAME_USE.SidTypeUser:
                    rv.ObjectClass = Strings.ObjectClassUser;
                    rv.ObjectClass = Strings.ObjectClassOther;
        /// Indicate whether a Status code is a successful value.
        /// <param name="ntStatus">
        /// One of the NTSTATUS code values indicating the error, if any.
        /// True if the Status code represents a success, false otherwise.
        private static bool Succeeded(UInt32 ntStatus)
            return NtStatus.IsSuccess(ntStatus);
        /// Helper to throw an exception if the provided Status code
        /// represents a failure.
        /// A <see cref="Context"/> object containing information about the
        /// current operation. If this parameter is null, the class's context
        /// is used.
        private void ThrowOnFailure(UInt32 ntStatus, Context context = null)
            if (NtStatus.IsError(ntStatus))
                var ex = MakeException(ntStatus, context);
                if (ex != null)
                    throw ex;
        /// Create an appropriate exception from the specified status code.
        /// An <see cref="Exception"/> object, or an object derived from Exception,
        /// appropriate to the error. If <paramref name="ntStatus"/> does not
        /// indicate an error, the method returns null.
        private Exception MakeException(UInt32 ntStatus, Context context = null)
            if (!NtStatus.IsError(ntStatus))
                context = this.context;
            switch (ntStatus)
                case NtStatus.STATUS_ACCESS_DENIED:
                    return new AccessDeniedException(context.target);
                case NtStatus.STATUS_INVALID_ACCOUNT_NAME:
                    return new InvalidNameException(context.ObjectName, context.target);
                case NtStatus.STATUS_USER_EXISTS:
                    if (context.operation == ContextOperation.New &&
                        context.type == ContextObjectType.User)
                        return new UserExistsException(context.ObjectName, context.target);
                        return new NameInUseException(context.ObjectName, context.target);
                case NtStatus.STATUS_ALIAS_EXISTS:
                        context.type == ContextObjectType.Group)
                        return new GroupExistsException(context.ObjectName, context.target);
                case NtStatus.STATUS_GROUP_EXISTS:
                case NtStatus.STATUS_NO_SUCH_ALIAS:
                case NtStatus.STATUS_NO_SUCH_GROUP:
                    return new GroupNotFoundException(context.ObjectName, context.target);
                case NtStatus.STATUS_NO_SUCH_USER:
                    return new UserNotFoundException(context.ObjectName, context.target);
                case NtStatus.STATUS_SPECIAL_GROUP:     // The group specified is a special group and cannot be operated on in the requested fashion.
                // case NtStatus.STATUS_SPECIAL_ALIAS: // referred to in source for SAM api, but not in ntstatus.h!!!
                    return new InvalidOperationException(StringUtil.Format(Strings.InvalidForGroup, context.ObjectName));
                case NtStatus.STATUS_SPECIAL_USER:  // The user specified is a special user and cannot be operated on in the requested fashion.
                    return new InvalidOperationException(StringUtil.Format(Strings.InvalidForUser, context.ObjectName));
                case NtStatus.STATUS_NO_SUCH_MEMBER:
                    return new MemberNotFoundException(context.MemberName, context.ObjectName);
                case NtStatus.STATUS_MEMBER_IN_ALIAS:
                case NtStatus.STATUS_MEMBER_IN_GROUP:
                    if (context.operation == ContextOperation.Remove &&
                        return new InvalidOperationException(StringUtil.Format(Strings.GroupHasMembers, context.ObjectName));
                        return new MemberExistsException(context.MemberName, context.ObjectName, context.target);
                case NtStatus.STATUS_MEMBER_NOT_IN_ALIAS:
                case NtStatus.STATUS_MEMBER_NOT_IN_GROUP:
                case NtStatus.STATUS_MEMBERS_PRIMARY_GROUP:
                    return new InvalidOperationException(StringUtil.Format(Strings.MembersPrimaryGroup, context.ObjectName));
                case NtStatus.STATUS_LAST_ADMIN:    // Cannot delete the last administrator.
                    return new InvalidOperationException(Strings.LastAdmin);
                case NtStatus.STATUS_ILL_FORMED_PASSWORD:
                case NtStatus.STATUS_PASSWORD_RESTRICTION:
                    return new InvalidPasswordException(Native.Win32.RtlNtStatusToDosError(ntStatus));
                // TODO: do we want to handle these?
                //      they appear to be returned only in functions we are not calling
                case NtStatus.STATUS_INVALID_SID:       // member sid is corrupted
                case NtStatus.STATUS_INVALID_MEMBER:    // member has wrong account type
                    return new InternalException(ntStatus, context.target);
        /// Create a DateTime object from a 64-bit value from one of the SAM
        /// structures.
        /// <param name="samValue">
        /// A signed 64-bit value representing a date and time.
        /// A nullable DateTime object representing a date and time,
        /// or null if the <paramref name="samValue"/> parameter is zero.
        private static DateTime? DateTimeFromSam(Int64 samValue)
            if (samValue == 0 || samValue == 0X7FFFFFFFFFFFFFFF)
            return DateTime.FromFileTime(samValue);
        /// Determine the source of a user or group. Either local, Active Directory,
        /// or Azure AD.
        /// A <see cref="SecurityIdentifier"/> object identifying the user or group.
        /// One of the <see cref="PrincipalSource"/> enumerations identifying the
        /// source of the object.
        private PrincipalSource? GetPrincipalSource(SecurityIdentifier sid)
            var bSid = new byte[sid.BinaryLength];
            var type = LSA_USER_ACCOUNT_TYPE.UnknownUserAccountType;
            // Use LsaLookupUserAccountType for Windows 10 and later.
            // Earlier versions of the OS will leave the property NULL because
            // it is too error prone to attempt to replicate the decisions of
            // LsaLookupUserAccountType.
            var os = GetOperatingSystem();
            if (os.Version.Major >= 10)
                UInt32 status = Native.Win32.LsaLookupUserAccountType(bSid, out type);
                if (NtStatus.IsError(status))
                    type = LSA_USER_ACCOUNT_TYPE.UnknownUserAccountType;
                    case LSA_USER_ACCOUNT_TYPE.ExternalDomainUserAccountType:
                    case LSA_USER_ACCOUNT_TYPE.PrimaryDomainUserAccountType:
                        return PrincipalSource.ActiveDirectory;
                    case LSA_USER_ACCOUNT_TYPE.LocalUserAccountType:
                        return PrincipalSource.Local;
                    case LSA_USER_ACCOUNT_TYPE.AADUserAccountType:
                        return PrincipalSource.AzureAD;
                    // Currently, there is no value returned by LsaLookupUserAccountType
                    // that corresponds to LSA_USER_ACCOUNT_TYPE.MSAUserAccountType,
                    // but there may be in the future, so we'll account for it here.
                    case LSA_USER_ACCOUNT_TYPE.MSAUserAccountType:
                    case LSA_USER_ACCOUNT_TYPE.LocalConnectedUserAccountType:
                        return PrincipalSource.MicrosoftAccount;
                    case LSA_USER_ACCOUNT_TYPE.InternetUserAccountType:
                        return sid.IsMsaAccount()
                            ? PrincipalSource.MicrosoftAccount
                            : PrincipalSource.Unknown;
                    case LSA_USER_ACCOUNT_TYPE.UnknownUserAccountType:
                        return PrincipalSource.Unknown;
        /// An <see cref="AccountInfo"/> object containing information about the
        /// user or group.
        private PrincipalSource? GetPrincipalSource(AccountInfo info)
            return GetPrincipalSource(info.Sid);
        /// A <see cref="SamRidEnumeration"/> object identifying the user or group.
        private PrincipalSource? GetPrincipalSource(SamRidEnumeration sre)
            return GetPrincipalSource(RidToSid(sre.domainHandle, sre.RelativeId));
        internal struct OSVERSIONINFOEX
            // The OSVersionInfoSize field must be set to Marshal.SizeOf(this)
            public int OSVersionInfoSize;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public ushort ServicePackMajor;
            public ushort ServicePackMinor;
            public short SuiteMask;
            public byte ProductType;
            public byte Reserved;
        [DllImport(PInvokeDllNames.GetVersionExDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetVersionEx(ref OSVERSIONINFOEX osVerEx);
        private static volatile OperatingSystem localOs;
        /// It only contains the properties that get used in powershell.
        internal sealed class OperatingSystem
            private Version _version;
            private string _servicePack;
            private string _versionString;
            internal OperatingSystem(Version version, string servicePack)
                ArgumentNullException.ThrowIfNull(version);
                _servicePack = servicePack;
            /// OS version.
            public Version Version
                get { return _version; }
            /// VersionString.
            public string VersionString
                    if (_versionString != null)
                        return _versionString;
                    // It's always 'VER_PLATFORM_WIN32_NT' for NanoServer and IoT
                    const string os = "Microsoft Windows NT ";
                    if (string.IsNullOrEmpty(_servicePack))
                        _versionString = os + _version.ToString();
                        _versionString = os + _version.ToString(3) + " " + _servicePack;
        // Wraps calls to acquire the OperatingSystem version
        private OperatingSystem GetOperatingSystem()
            if (localOs == null)
                OSVERSIONINFOEX osviex = new OSVERSIONINFOEX();
                osviex.OSVersionInfoSize = Marshal.SizeOf(osviex);
                if (!GetVersionEx(ref osviex))
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                Version ver = new Version(osviex.MajorVersion, osviex.MinorVersion, osviex.BuildNumber, (osviex.ServicePackMajor << 16) | osviex.ServicePackMinor);
                localOs = new OperatingSystem(ver, osviex.CSDVersion);
            return localOs;
            return Environment.OSVersion;
#endregion Utility Methods
#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
            if (!disposedValue)
                    // no managed objects need disposing.
                if (builtinDomainHandle != IntPtr.Zero)
                    status = SamApi.SamCloseHandle(builtinDomainHandle);
                    builtinDomainHandle = IntPtr.Zero;
                if (localDomainHandle != IntPtr.Zero)
                    status = SamApi.SamCloseHandle(localDomainHandle);
                    localDomainHandle = IntPtr.Zero;
                if (samHandle != IntPtr.Zero)
                    status = SamApi.SamCloseHandle(samHandle);
                    samHandle = IntPtr.Zero;
                    // Do nothing to satisfy CA1806: Do not ignore method results. We want the dispose to proceed regardless of the handle close status.
                disposedValue = true;
        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Sam()
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        // This code added to correctly implement the disposable pattern.
            // uncomment the following line if the finalizer is overridden above.
#endregion IDisposable Support

// ndp\clr\src\BCL\Microsoft\Win32\RegistryKey.cs.
  Note on transaction support:
  Eventually we will want to add support for NT's transactions to our
  TransactedRegistryKey API's (possibly Whidbey M3?).  When we do this, here's
  the list of API's we need to make transaction-aware:
  RegCreateKeyEx
  RegDeleteKey
  RegDeleteValue
  RegEnumKeyEx
  RegEnumValue
  RegOpenKeyEx
  RegQueryInfoKey
  RegQueryValueEx
  RegSetValueEx
  We can ignore RegConnectRegistry (remote registry access doesn't yet have
  transaction support) and RegFlushKey.  RegCloseKey doesn't require any
  additional work.  .
  Note on ACL support:
  The key thing to note about ACL's is you set them on a kernel object like a
  registry key, then the ACL only gets checked when you construct handles to
  them.  So if you set an ACL to deny read access to yourself, you'll still be
  able to read with that handle, but not with new handles.
  Another peculiarity is a Terminal Server app compatibility hack.  The OS
  will second guess your attempt to open a handle sometimes.  If a certain
  combination of Terminal Server app compat registry keys are set, then the
  OS will try to reopen your handle with lesser permissions if you couldn't
  open it in the specified mode.  So on some machines, we will see handles that
  may not be able to read or write to a registry key.  It's very strange.  But
  the real test of these handles is attempting to read or set a value in an
  affected registry key.
  For reference, at least two registry keys must be set to particular values
  for this behavior:
  HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\RegistryExtensionFlags, the least significant bit must be 1.
  HKLM\SYSTEM\CurrentControlSet\Control\TerminalServer\TSAppCompat must be 1
  There might possibly be an interaction with yet a third registry key as well.
    // Putting this in a separate internal class to avoid OACR warning DoNotDeclareReadOnlyMutableReferenceTypes.
    internal sealed class BaseRegistryKeys
        // We could use const here, if C# supported ELEMENT_TYPE_I fully.
        internal static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        internal static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        internal static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        internal static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
    /// Registry encapsulation. To get an instance of a TransactedRegistryKey use the
    /// Registry class's static members then call OpenSubKey.
    /// @see Registry
    /// @security(checkDllCalls=off)
    /// @security(checkClassLinking=on)
    // Suppressed because these objects are written to the pipeline so need to be accessible.
    public sealed class TransactedRegistryKey : MarshalByRefObject, IDisposable
        // Dirty indicates that we have munged data that should be potentially
        // written to disk.
        private const int STATE_DIRTY = 0x0001;
        // SystemKey indicates that this is a "SYSTEMKEY" and shouldn't be "opened"
        // or "closed".
        private const int STATE_SYSTEMKEY = 0x0002;
        // Access
        private const int STATE_WRITEACCESS = 0x0004;
        // Names of keys.  This array must be in the same order as the HKEY values listed above.
        private static readonly string[] s_hkeyNames = new string[] {
                "HKEY_PERFORMANCE_DATA",
                "HKEY_DYN_DATA"
        // MSDN defines the following limits for registry key names & values:
        // Key Name: 255 characters
        // Value name:  Win9x: 255 NT: 16,383 Unicode characters, or 260 ANSI chars
        // Value: either 1 MB or current available memory, depending on registry format.
        private const int MaxKeyLength = 255;
        private const int MaxValueNameLength = 16383;
        private const int MaxValueDataLength = 1024 * 1024;
        private SafeRegistryHandle _hkey = null;
        private int _state = 0;
        private string _keyName;
        private RegistryKeyPermissionCheck _checkMode;
        private System.Transactions.Transaction _myTransaction;
        private SafeTransactionHandle _myTransactionHandle;
        // This is a wrapper around RegOpenKeyTransacted that implements a workaround
        // to TxF bug number 181242 After calling RegOpenKeyTransacted, it calls RegQueryInfoKey.
        // If that call fails with ERROR_INVALID_TRANSACTION, we have possibly run into bug 181242. To workaround
        // this, we open the key without a transaction and then open it again with
        // a transaction and return THAT hkey.
        private int RegOpenKeyTransactedWrapper(SafeRegistryHandle hKey, string lpSubKey,
                    int ulOptions, int samDesired, out SafeRegistryHandle hkResult,
                    SafeTransactionHandle hTransaction, IntPtr pExtendedParameter)
            int error = Win32Native.ERROR_SUCCESS;
            SafeRegistryHandle hKeyToReturn = null;
            error = Win32Native.RegOpenKeyTransacted(_hkey, lpSubKey, ulOptions, samDesired, out hKeyToReturn, hTransaction, pExtendedParameter);
            if (Win32Native.ERROR_SUCCESS == error && !hKeyToReturn.IsInvalid)
                // This is a check and workaround for TxR bug 181242. If we try to use the transacted hKey we just opened
                // for a call to RegQueryInfoKey and get back a ERROR_INVALID_TRANSACTION error, then the key might be a symbolic link and TxR didn't
                // do the open correctly. The workaround is to open it non-transacted, then open it again transacted without
                // a subkey string. If we get some error other than ERROR_INVALID_TRANSACTION from RegQueryInfoKey, just ignore it for now.
                int subkeyCount = 0;
                int valueCount = 0;
                error = Win32Native.RegQueryInfoKey(hKeyToReturn,
                                          Win32Native.NULL,
                                          ref subkeyCount,  // subkeys
                                          ref valueCount,     // values
                if (Win32Native.ERROR_INVALID_TRANSACTION == error)
                    SafeRegistryHandle nonTxKey = null;
                    SafeRegistryHandle txKey = null;
                    error = Win32Native.RegOpenKeyEx(_hkey, lpSubKey, ulOptions, samDesired, out nonTxKey);
                    // If we got some error on this open, just ignore it and continue on with the handle
                    // we got on the original RegOpenKeyTransacted.
                    if (Win32Native.ERROR_SUCCESS == error)
                        // Now do an RegOpenKeyTransacted with the non-transacted key and no "subKey" parameter.
                        error = Win32Native.RegOpenKeyTransacted(nonTxKey, null, ulOptions, samDesired, out txKey, hTransaction, pExtendedParameter);
                            // Let's use this hkey instead.
                            hKeyToReturn.Dispose();
                            hKeyToReturn = txKey;
                        nonTxKey.Dispose();
                        nonTxKey = null;
            hkResult = hKeyToReturn;
            return error;
         * Creates a TransactedRegistryKey.
         * This key is bound to hkey, if writable is <b>false</b> then no write operations
         * will be allowed. If systemkey is set then the hkey won't be released
         * when the object is GC'ed.
        private TransactedRegistryKey(SafeRegistryHandle hkey, bool writable, bool systemkey,
                                      System.Transactions.Transaction transaction, SafeTransactionHandle txHandle)
            _hkey = hkey;
            _keyName = string.Empty;
            if (systemkey)
                _state |= STATE_SYSTEMKEY;
            if (writable)
                _state |= STATE_WRITEACCESS;
            // We want to take our own clone so we can dispose it when we want and
            // aren't susceptible to the caller disposing it.
            if (transaction != null)
                _myTransaction = transaction.Clone();
                _myTransactionHandle = txHandle;
                _myTransaction = null;
                _myTransactionHandle = null;
        private SafeTransactionHandle GetTransactionHandle()
            SafeTransactionHandle safeTransactionHandle = null;
            // If myTransaction is not null and is not the same as Transaction.Current
            // this is an invalid operation. The transaction within which the RegistryKey object was created
            // needs to be the same as the transaction being used now.
            if (_myTransaction != null)
                if (!_myTransaction.Equals(Transaction.Current))
                    throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_MustUseSameTransaction);
                    safeTransactionHandle = _myTransactionHandle;
            else  // we want to use Transaction.Current for the transaction.
                safeTransactionHandle = SafeTransactionHandle.Create();
            return safeTransactionHandle;
        /// <summary>TransactedRegistryKey.Close
        /// <para>Closes this key, flushes it to disk if the contents have been modified.
        /// Utilizes Transaction.Current for its transaction.</para>
            if (_hkey != null)
                if (!IsSystemKey())
                        _hkey.Dispose();
                        // we don't really care if the handle is invalid at this point
                        _hkey = null;
                // Dispose the transaction because we cloned it.
                    _myTransaction.Dispose();
                catch (TransactionException)
                    // ignore.
        /// <summary>TransactedRegistryKey.Flush
        /// <para>Flushes this key. Utilizes Transaction.Current for its transaction.</para>
        public void Flush()
            // Require a transaction. This will throw for "Base" keys because they aren't associated with a transaction.
            VerifyTransaction();
                if (IsDirty())
                    int ret = Win32Native.RegFlushKey(_hkey);
                    if (Win32Native.ERROR_SUCCESS != ret)
                        throw new IOException(Win32Native.GetMessage(ret), ret);
        /// <summary>TransactedRegistryKey.Dispose
        /// <para>Disposes this key. Utilizes Transaction.Current for its transaction.</para>
        /// <para>Creates a new subkey, or opens an existing one.
        /// <param name='subkey'>Name or path to subkey to create or open. Cannot be null or an empty string,
        /// otherwise an ArgumentException is thrown.</param>
        /// <returns>A TransactedRegistryKey object for the subkey, which is associated with Transaction.Current.
        /// returns null if the operation failed.</returns>
        // Suppressed to be consistent with naming in Microsoft.Win32.RegistryKey
            return CreateSubKey(subkey, _checkMode);
        /// <param name='permissionCheck'>One of the Microsoft.Win32.RegistryKeyPermissionCheck values that
        /// specifies whether the key is opened for read or read/write access.</param>
        public TransactedRegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck)
            return CreateSubKeyInternal(subkey, permissionCheck, (TransactedRegistrySecurity)null);
        /// <param name='registrySecurity'>A TransactedRegistrySecurity object that specifies the access control security for the new key.</param>
        public unsafe TransactedRegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, TransactedRegistrySecurity registrySecurity)
            return CreateSubKeyInternal(subkey, permissionCheck, registrySecurity);
        private unsafe TransactedRegistryKey CreateSubKeyInternal(string subkey, RegistryKeyPermissionCheck permissionCheck, object registrySecurityObj)
            ValidateKeyName(subkey);
            // RegCreateKeyTransacted requires a non-empty key name, so let's deal with that here.
            if (string.Empty == subkey)
                throw new ArgumentException(RegistryProviderStrings.Arg_RegKeyStrEmpty);
            ValidateKeyMode(permissionCheck);
            EnsureWriteable();
            subkey = FixupName(subkey); // Fixup multiple slashes to a single slash
            // only keys opened under read mode is not writable
            TransactedRegistryKey existingKey = InternalOpenSubKey(subkey, (permissionCheck != RegistryKeyPermissionCheck.ReadSubTree));
            if (existingKey != null)
            { // Key already exits
                CheckSubKeyWritePermission(subkey);
                CheckSubTreePermission(subkey, permissionCheck);
                existingKey._checkMode = permissionCheck;
                return existingKey;
            CheckSubKeyCreatePermission(subkey);
            Win32Native.SECURITY_ATTRIBUTES secAttrs = null;
            TransactedRegistrySecurity registrySecurity = registrySecurityObj as TransactedRegistrySecurity;
            // For ACL's, get the security descriptor from the RegistrySecurity.
            if (registrySecurity != null)
                secAttrs = new Win32Native.SECURITY_ATTRIBUTES();
                secAttrs.nLength = (int)Marshal.SizeOf(secAttrs);
                byte[] sd = registrySecurity.GetSecurityDescriptorBinaryForm();
                // We allocate memory on the stack to improve the speed.
                // So this part of code can't be refactored into a method.
                byte* pSecDescriptor = stackalloc byte[sd.Length];
                Microsoft.PowerShell.Commands.Internal.Buffer.memcpy(sd, 0, pSecDescriptor, 0, sd.Length);
                secAttrs.pSecurityDescriptor = pSecDescriptor;
            int disposition = 0;
            // By default, the new key will be writable.
            SafeRegistryHandle result = null;
            int ret = 0;
            SafeTransactionHandle safeTransactionHandle = GetTransactionHandle();
            ret = Win32Native.RegCreateKeyTransacted(_hkey,
                subkey,
                GetRegistryKeyAccess(permissionCheck != RegistryKeyPermissionCheck.ReadSubTree),
                secAttrs,
                out disposition,
                safeTransactionHandle,
                IntPtr.Zero
            if (ret == 0 && !result.IsInvalid)
                TransactedRegistryKey key = new TransactedRegistryKey(result, (permissionCheck != RegistryKeyPermissionCheck.ReadSubTree), false,
                                                                      Transaction.Current, safeTransactionHandle);
                key._checkMode = permissionCheck;
                if (subkey.Length == 0)
                    key._keyName = _keyName;
                    key._keyName = _keyName + "\\" + subkey;
            else if (ret != 0) // syscall failed, ret is an error code.
                Win32Error(ret, _keyName + "\\" + subkey);  // Access denied?
            BCLDebug.Assert(false, "Unexpected code path in RegistryKey::CreateSubKey");
        /// <para>Deletes the specified subkey. Will throw an exception if the subkey has
        /// subkeys. To delete a tree of subkeys use, DeleteSubKeyTree.
        /// <exception cref="InvalidOperationException">Thrown if the subkey as child subkeys.</exception>
        /// <param name='subkey'>The subkey to delete.</param>
        public void DeleteSubKey(string subkey)
            DeleteSubKey(subkey, true);
        /// <exception cref="ArgumentException">Thrown if true is specified for throwOnMissingSubKey and the
        /// specified subkey does not exist.</exception>
        /// <param name='throwOnMissingSubKey'>Specify true if an ArgumentException should be thrown if
        /// the specified subkey does not exist. If false is specified, a missing subkey does not throw
        /// an exception.</param>
        public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
            // Open the key we are deleting and check for children. Be sure to
            // explicitly call close to avoid keeping an extra HKEY open.
            TransactedRegistryKey key = InternalOpenSubKey(subkey, false);
                    if (key.InternalSubKeyCount() > 0)
                        throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_RegRemoveSubKey);
                ret = Win32Native.RegDeleteKeyTransacted(_hkey, subkey, 0, 0, safeTransactionHandle, IntPtr.Zero);
                if (ret != 0)
                    if (ret == Win32Native.ERROR_FILE_NOT_FOUND)
                        if (throwOnMissingSubKey)
                            throw new ArgumentException(RegistryProviderStrings.ArgumentException_RegSubKeyAbsent);
                        Win32Error(ret, null);
            { // there is no key which also means there is no subkey
        /// <para>Recursively deletes a subkey and any child subkeys.
        /// <param name="subkey">The subkey to delete.</param>
            // Security concern: Deleting a hive's "" subkey would delete all
            // of that hive's contents.  Don't allow "".
            if ((string.IsNullOrEmpty(subkey) || subkey.Length == 0) && IsSystemKey())
                throw new ArgumentException(RegistryProviderStrings.ArgRegKeyDelHive);
            CheckSubTreeWritePermission(subkey);
            TransactedRegistryKey key = InternalOpenSubKey(subkey, true);
                        string[] keys = key.InternalGetSubKeyNames();
                        for (int i = 0; i < keys.Length; i++)
                            key.DeleteSubKeyTreeInternal(keys[i]);
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSubKeyAbsent);
        // An internal version which does no security checks or argument checking.  Skipping the
        // security checks should give us a slight perf gain on large trees.
        private void DeleteSubKeyTreeInternal(string subkey)
        /// <para>Deletes the specified value from this key.
        /// <param name="name">Name of the value to delete.</param>
            DeleteValue(name, true);
        /// <param name="throwOnMissingValue">Specify true if an ArgumentException should be thrown if
        /// the specified value does not exist. If false is specified, a missing value does not throw
        public void DeleteValue(string name, bool throwOnMissingValue)
            CheckValueWritePermission(name);
            int errorCode = Win32Native.RegDeleteValue(_hkey, name);
            // From windows 2003 server, if the name is too long we will get error code ERROR_FILENAME_EXCED_RANGE
            // This still means the name doesn't exist. We need to be consistent with previous OS.
            if (errorCode == Win32Native.ERROR_FILE_NOT_FOUND || errorCode == Win32Native.ERROR_FILENAME_EXCED_RANGE)
                if (throwOnMissingValue)
                    throw new ArgumentException(RegistryProviderStrings.Arg_RegSubKeyValueAbsent);
                    errorCode = Win32Native.ERROR_SUCCESS;
            if (Win32Native.ERROR_SUCCESS != errorCode)
                Win32Error(errorCode, null);
         * Retrieves a new TransactedRegistryKey that represents the requested key. Valid
         * values are:
         * HKEY_CLASSES_ROOT,
         * HKEY_CURRENT_USER,
         * HKEY_LOCAL_MACHINE,
         * HKEY_USERS,
         * HKEY_PERFORMANCE_DATA,
         * HKEY_CURRENT_CONFIG,
         * HKEY_DYN_DATA.
         * @param hKey HKEY_* to open.
         * @return the TransactedRegistryKey requested.
        internal static TransactedRegistryKey GetBaseKey(IntPtr hKey)
            int index = ((int)hKey) & 0x0FFFFFFF;
            BCLDebug.Assert(index >= 0 && index < s_hkeyNames.Length, "index is out of range!");
            BCLDebug.Assert((((int)hKey) & 0xFFFFFFF0) == 0x80000000, "Invalid hkey value!");
            SafeRegistryHandle srh = new SafeRegistryHandle(hKey, false);
            // For Base keys, there is no transaction associated with the HKEY.
            TransactedRegistryKey key = new TransactedRegistryKey(srh, true, true, null, null);
            key._checkMode = RegistryKeyPermissionCheck.Default;
            key._keyName = s_hkeyNames[index];
        /// <para>Retrieves a subkey. If readonly is true, then the subkey is opened with
        /// read-only access.
        /// <returns>The subkey requested or null if the operation failed.</returns>
        /// <param name="name">Name or path of the subkey to open.</param>
        /// <param name="writable">Set to true of you only need readonly access.</param>
            ValidateKeyName(name);
            EnsureNotDisposed();
            name = FixupName(name); // Fixup multiple slashes to a single slash
            CheckOpenSubKeyPermission(name, writable);
            ret = RegOpenKeyTransactedWrapper(_hkey, name, 0, GetRegistryKeyAccess(writable), out result, safeTransactionHandle, IntPtr.Zero);
                TransactedRegistryKey key = new TransactedRegistryKey(result, writable, false, Transaction.Current, safeTransactionHandle);
                key._checkMode = GetSubKeyPermissionCheck(writable);
                key._keyName = _keyName + "\\" + name;
            // Return null if we didn't find the key.
            if (ret == Win32Native.ERROR_ACCESS_DENIED || ret == Win32Native.ERROR_BAD_IMPERSONATION_LEVEL)
                // We need to throw SecurityException here for compatibility reasons,
                // although UnauthorizedAccessException will make more sense.
                throw new SecurityException(RegistryProviderStrings.Security_RegistryPermission);
        /// <para>Retrieves a subkey.
        /// <param name="permissionCheck">One of the Microsoft.Win32.RegistryKeyPermissionCheck values that specifies
        /// whether the key is opened for read or read/write access.</param>
        public TransactedRegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck)
            return InternalOpenSubKey(name, permissionCheck, GetRegistryKeyAccess(permissionCheck));
        /// <param name="rights">A bitwise combination of Microsoft.Win32.RegistryRights values that specifies the desired security access.</param>
        public TransactedRegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, RegistryRights rights)
            return InternalOpenSubKey(name, permissionCheck, (int)rights);
        private TransactedRegistryKey InternalOpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, int rights)
            ValidateKeyRights(rights);
            CheckOpenSubKeyPermission(name, permissionCheck);
            ret = RegOpenKeyTransactedWrapper(_hkey, name, 0, rights, out result, safeTransactionHandle, IntPtr.Zero);
                TransactedRegistryKey key = new TransactedRegistryKey(result, (permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree), false,
                // We need to throw SecurityException here for compatibility reason,
        // This required no security checks. This is to get around the Deleting SubKeys which only require
        // write permission. They call OpenSubKey which required read. Now instead call this function w/o security checks
        internal TransactedRegistryKey InternalOpenSubKey(string name, bool writable)
            int winAccess = GetRegistryKeyAccess(writable);
            ret = RegOpenKeyTransactedWrapper(_hkey, name, 0, winAccess, out result, safeTransactionHandle, IntPtr.Zero);
        /// <para>Retrieves a subkey for readonly access.
        public TransactedRegistryKey OpenSubKey(string name)
            return OpenSubKey(name, false);
        /// <para>Retrieves the count of subkeys.
        /// <returns>The count of subkeys.</returns>
                CheckKeyReadPermission();
                return InternalSubKeyCount();
        internal int InternalSubKeyCount()
            // Don't require a transaction. We don't want to throw for "Base" keys.
            int subkeys = 0;
            int junk = 0;
            int ret = Win32Native.RegQueryInfoKey(_hkey,
                                      ref subkeys,  // subkeys
                                      ref junk,     // values
            return subkeys;
        /// <para>Retrieves an array of strings containing all the subkey names.
        /// <returns>A string array containing all the subkey names.</returns>
            return InternalGetSubKeyNames();
        internal string[] InternalGetSubKeyNames()
            int subkeys = InternalSubKeyCount();
            string[] names = new string[subkeys];  // Returns 0-length array if empty.
            if (subkeys > 0)
                StringBuilder name = new StringBuilder(256);
                int namelen;
                for (int i = 0; i < subkeys; i++)
                    namelen = name.Capacity; // Don't remove this. The API's doesn't work if this is not properly initialised.
                    int ret = Win32Native.RegEnumKeyEx(_hkey,
                        out namelen,
                    names[i] = name.ToString();
        /// <para>Retrieves the count of values.
        /// <returns>A count of values.</returns>
        public int ValueCount
                return InternalValueCount();
        internal int InternalValueCount()
            int values = 0;
                                      ref junk,     // subkeys
                                      ref values,   // values
        /// <para>Retrieves an array of strings containing all the value names.
        /// <returns>All the value names.</returns>
            int values = InternalValueCount();
            string[] names = new string[values];
            if (values > 0)
                int currentlen;
                int ret;
                for (int i = 0; i < values; i++)
                    currentlen = name.Capacity;
                    ret = Win32Native.ERROR_MORE_DATA;
                    // loop while we get error_more_data or until we have exceeded
                    // the max name length.
                    while (Win32Native.ERROR_MORE_DATA == ret)
                        namelen = currentlen;
                        ret = Win32Native.RegEnumValue(_hkey,
                            ref namelen,
                            if (ret != Win32Native.ERROR_MORE_DATA)
                            // We got ERROR_MORE_DATA. Let's see if we can make the buffer
                            // bigger.
                            if (MaxValueNameLength == currentlen)
                            currentlen = currentlen * 2;
                            if (MaxValueNameLength < currentlen)
                                currentlen = MaxValueNameLength;
                            // Allocate a new buffer.
                            name = new StringBuilder(currentlen);
        /// <para>Retrieves the specified value. null is returned if the value
        /// doesn't exist. Utilizes Transaction.Current for its transaction.</para>
        /// <para>Note that name can be null or "", at which point the
        /// unnamed or default value of this Registry key is returned, if any.</para>
        /// <returns>The data associated with the value.</returns>
        /// <param name="name">Name of value to retrieve.</param>
            CheckValueReadPermission(name);
            return InternalGetValue(name, null, false, true);
        /// <param name="defaultValue">Value to return if name doesn't exist.</param>
            return InternalGetValue(name, defaultValue, false, true);
        /// <param name="options">One of the Microsoft.Win32.RegistryValueOptions values that specifies
        /// optional processing of the retrieved value.</param>
            if (options < RegistryValueOptions.None || options > RegistryValueOptions.DoNotExpandEnvironmentNames)
                string resourceTemplate = RegistryProviderStrings.Arg_EnumIllegalVal;
                string resource = string.Format(CultureInfo.CurrentCulture, resourceTemplate, options.ToString());
                throw new ArgumentException(resource);
            bool doNotExpand = (options == RegistryValueOptions.DoNotExpandEnvironmentNames);
            return InternalGetValue(name, defaultValue, doNotExpand, true);
        internal object InternalGetValue(string name, object defaultValue, bool doNotExpand, bool checkSecurity)
            if (checkSecurity)
                // Name can be null!  It's the most common use of RegQueryValueEx
            object data = defaultValue;
            int type = 0;
            int datasize = 0;
            int ret = Win32Native.RegQueryValueEx(_hkey, name, null, ref type, (byte[])null, ref datasize);
                // For stuff like ERROR_FILE_NOT_FOUND, we want to return null (data).
                // Some OS's returned ERROR_MORE_DATA even in success cases, so we
                // want to continue on through the function.
                case Win32Native.REG_DWORD_BIG_ENDIAN:
                case Win32Native.REG_BINARY:
                        byte[] blob = new byte[datasize];
                        ret = Win32Native.RegQueryValueEx(_hkey, name, null, ref type, blob, ref datasize);
                        data = blob;
                case Win32Native.REG_QWORD:
                    {    // also REG_QWORD_LITTLE_ENDIAN
                        if (datasize > 8)
                            // prevent an AV in the edge case that datasize is larger than sizeof(long)
                            goto case Win32Native.REG_BINARY;
                        long blob = 0;
                        BCLDebug.Assert(datasize == 8, "datasize==8");
                        // Here, datasize must be 8 when calling this
                        ret = Win32Native.RegQueryValueEx(_hkey, name, null, ref type, ref blob, ref datasize);
                case Win32Native.REG_DWORD:
                    {    // also REG_DWORD_LITTLE_ENDIAN
                        if (datasize > 4)
                            // prevent an AV in the edge case that datasize is larger than sizeof(int)
                            goto case Win32Native.REG_QWORD;
                        int blob = 0;
                        BCLDebug.Assert(datasize == 4, "datasize==4");
                        // Here, datasize must be four when calling this
                case Win32Native.REG_SZ:
                        StringBuilder blob = new StringBuilder(datasize / 2);
                        data = blob.ToString();
                case Win32Native.REG_EXPAND_SZ:
                        if (doNotExpand)
                            data = Environment.ExpandEnvironmentVariables(blob.ToString());
                case Win32Native.REG_MULTI_SZ:
                        IList<string> strings = new List<string>();
                        char[] blob = new char[datasize / 2];
                        int cur = 0;
                        int len = blob.Length;
                        while (ret == 0 && cur < len)
                            int nextNull = cur;
                            while (nextNull < len && blob[nextNull] != (char)0)
                                nextNull++;
                            if (nextNull < len)
                                BCLDebug.Assert(blob[nextNull] == (char)0, "blob[nextNull] should be 0");
                                if (nextNull - cur > 0)
                                    strings.Add(new string(blob, cur, nextNull - cur));
                                    // we found an empty string.  But if we're at the end of the data,
                                    // it's just the extra null terminator.
                                    if (nextNull != len - 1)
                                        strings.Add(string.Empty);
                                strings.Add(new string(blob, cur, len - cur));
                            cur = nextNull + 1;
                        data = new string[strings.Count];
                        strings.CopyTo((string[])data, 0);
                        // data = strings.GetAllItems(String.class);
                case Win32Native.REG_NONE:
                case Win32Native.REG_LINK:
        /// <para>Retrieves the registry data type of the value associated with the specified name.
        /// <returns>A RegistryValueKind value representing the registry data type of the value associated with name.</returns>
        /// <param name="name">The value name whose data type is to be retrieved.</param>
            if (!Enum.IsDefined(typeof(RegistryValueKind), type))
                return (RegistryValueKind)type;
         * Retrieves the current state of the dirty property.
         * A key is marked as dirty if any operation has occurred that modifies the
         * contents of the key.
         * @return <b>true</b> if the key has been modified.
        private bool IsDirty()
            return (_state & STATE_DIRTY) != 0;
        private bool IsSystemKey()
            return (_state & STATE_SYSTEMKEY) != 0;
        private bool IsWritable()
            return (_state & STATE_WRITEACCESS) != 0;
        /// <para>Retrieves the name of the key.</para>
        /// <returns>The name of the key.</returns>
                return _keyName;
        private void SetDirty()
            _state |= STATE_DIRTY;
        /// <para>Sets the specified value. Utilizes Transaction.Current for its transaction.</para>
        /// <param name="name">Name of value to store data in.</param>
        /// <param name="value">Data to store.</param>
            SetValue(name, value, RegistryValueKind.Unknown);
        /// <param name="valueKind">The registry data type to use when storing the data.</param>
        public unsafe void SetValue(string name, object value, RegistryValueKind valueKind)
            ArgumentNullException.ThrowIfNull(value, RegistryProviderStrings.Arg_Value);
            if (name != null && name.Length > MaxValueNameLength)
                throw new ArgumentException(RegistryProviderStrings.Arg_RegValueNameStrLenBug);
            if (!Enum.IsDefined(typeof(RegistryValueKind), valueKind))
                throw new ArgumentException(RegistryProviderStrings.Arg_RegBadKeyKind);
            if (ContainsRegistryValue(name))
            { // Existing key
            { // Creating a new value
                CheckValueCreatePermission(name);
            if (valueKind == RegistryValueKind.Unknown)
                // this is to maintain compatibility with the old way of autodetecting the type.
                // SetValue(string, object) will come through this codepath.
                valueKind = CalculateValueKind(value);
                            string data = value.ToString();
                            // divide by 2 to account for unicode.
                            if (MaxValueDataLength / 2 < data.Length)
                                throw new ArgumentException(RegistryProviderStrings.Arg_ValueDataLenBug);
                            ret = Win32Native.RegSetValueEx(_hkey,
                                valueKind,
                                data.Length * 2 + 2);
                            // Other thread might modify the input array after we calculate the buffer length.
                            // Make a copy of the input array to be safe.
                            string[] dataStrings = (string[])(((string[])value).Clone());
                            int sizeInBytes = 0;
                            // First determine the size of the array
                            for (int i = 0; i < dataStrings.Length; i++)
                                if (dataStrings[i] == null)
                                    throw new ArgumentException(RegistryProviderStrings.Arg_RegSetStrArrNull);
                                sizeInBytes += (dataStrings[i].Length + 1) * 2;
                            sizeInBytes += 2;
                            if (MaxValueDataLength < sizeInBytes)
                            byte[] basePtr = new byte[sizeInBytes];
                            fixed (byte* b = basePtr)
                                int totalBytesMoved = 0;
                                int currentBytesMoved = 0;
                                // Write out the strings...
                                    currentBytesMoved = System.Text.Encoding.Unicode.GetBytes(dataStrings[i], 0, dataStrings[i].Length, basePtr, totalBytesMoved);
                                    totalBytesMoved += currentBytesMoved;
                                    basePtr[totalBytesMoved] = 0;
                                    basePtr[totalBytesMoved + 1] = 0;
                                    totalBytesMoved += 2;
                                    RegistryValueKind.MultiString,
                                    basePtr,
                                    sizeInBytes);
                        byte[] dataBytes = (byte[])value;
                        if (MaxValueDataLength < dataBytes.Length)
                            RegistryValueKind.Binary,
                            dataBytes,
                            dataBytes.Length);
                            // We need to use Convert here because we could have a boxed type cannot be
                            // unboxed and cast at the same time.  I.e. ((int)(object)(short) 5) will fail.
                            int data = Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
                                RegistryValueKind.DWord,
                                ref data,
                                4);
                            long data = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
                                RegistryValueKind.QWord,
                                8);
                throw new ArgumentException(RegistryProviderStrings.Arg_RegSetMismatchedKind);
                SetDirty();
        private RegistryValueKind CalculateValueKind(object value)
            // This logic matches what used to be in SetValue(string name, object value) in the v1.0 and v1.1 days.
            // Even though we could add detection for an int64 in here, we want to maintain compatibility with the
            // old behavior.
            if (value is Int32)
                return RegistryValueKind.DWord;
            else if (value is Array)
                if (value is byte[])
                    return RegistryValueKind.Binary;
                else if (value is string[])
                    return RegistryValueKind.MultiString;
                    string resourceTemplate = RegistryProviderStrings.Arg_RegSetBadArrType;
                    string resource = string.Format(CultureInfo.CurrentCulture, resourceTemplate, value.GetType().Name);
                return RegistryValueKind.String;
         * Retrieves a string representation of this key.
         * @return a string representing the key.
        /// <para>Retrieves a string representation of this key.</para>
        /// <returns>A string representing the key.</returns>
        /// <para>Returns the access control security for the current registry key.
        /// <returns>A TransactedRegistrySecurity object that describes the access control
        /// permissions on the registry key represented by the current TransactedRegistryKey.</returns>
        public TransactedRegistrySecurity GetAccessControl()
            return GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
        /// <param name="includeSections">A bitwise combination of AccessControlSections values that specifies the type of security information to get.</param>
        public TransactedRegistrySecurity GetAccessControl(AccessControlSections includeSections)
            return new TransactedRegistrySecurity(_hkey, _keyName, includeSections);
        /// <para>Applies Windows access control security to an existing registry key.
        /// <param name="registrySecurity">A TransactedRegistrySecurity object that specifies the access control security to apply to the current subkey.</param>
        public void SetAccessControl(TransactedRegistrySecurity registrySecurity)
            ArgumentNullException.ThrowIfNull(registrySecurity);
            registrySecurity.Persist(_hkey, _keyName);
         * After calling GetLastWin32Error(), it clears the last error field,
         * so you must save the HResult and pass it to this method.  This method
         * will determine the appropriate exception to throw dependent on your
         * error, and depending on the error, insert a string into the message
         * gotten from the ResourceManager.
        internal void Win32Error(int errorCode, string str)
                case Win32Native.ERROR_ACCESS_DENIED:
                    if (str != null)
                        string resourceTemplate = RegistryProviderStrings.UnauthorizedAccess_RegistryKeyGeneric_Key;
                        string resource = string.Format(CultureInfo.CurrentCulture, resourceTemplate, str);
                        throw new UnauthorizedAccessException(resource);
                case Win32Native.ERROR_INVALID_HANDLE:
                    // * For normal RegistryKey instances we dispose the SafeRegHandle and throw IOException.
                    // * However, for HKEY_PERFORMANCE_DATA (on a local or remote machine) we avoid disposing the
                    // * SafeRegHandle and only throw the IOException.  This is to workaround reentrancy issues
                    // * in PerformanceCounter.NextValue() where the API could throw {NullReference, ObjectDisposed, ArgumentNull}Exception
                    // * on reentrant calls because of this error code path in RegistryKey
                    // *
                    // * Normally we'd make our caller synchronize access to a shared RegistryKey instead of doing something like this,
                    // * however we shipped PerformanceCounter.NextValue() un-synchronized in v2.0RTM and customers have taken a dependency on
                    // * this behavior (being able to simultaneously query multiple remote-machine counters on multiple threads, instead of
                    // * having serialized access).
                    // * FUTURE: Consider changing PerformanceCounterLib to handle its own Win32 RegistryKey API calls instead of depending
                    // * on Microsoft.Win32.RegistryKey, so that RegistryKey can be clean of special-cases for HKEY_PERFORMANCE_DATA.
                    _hkey.SetHandleAsInvalid();
                case Win32Native.ERROR_FILE_NOT_FOUND:
                        string resourceTemplate = RegistryProviderStrings.Arg_RegKeyNotFound;
                        string resource = string.Format(CultureInfo.CurrentCulture, resourceTemplate, errorCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        throw new IOException(resource);
                    throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
        internal static void Win32ErrorStatic(int errorCode, string str)
        internal static string FixupName(string name)
            BCLDebug.Assert(name != null, "[FixupName]name!=null");
            if (name.Contains('\\'))
            StringBuilder sb = new StringBuilder(name);
            FixupPath(sb);
            int temp = sb.Length - 1;
            if (sb[temp] == '\\') // Remove trailing slash
                sb.Length = temp;
        private static void FixupPath(StringBuilder path)
            int length = path.Length;
            bool fixup = false;
            char markerChar = (char)0xFFFF;
            while (i < length - 1)
                if (path[i] == '\\')
                    while (i < length)
                            path[i] = markerChar;
                            fixup = true;
            if (fixup)
                    if (path[i] == markerChar)
                    path[j] = path[i];
                path.Length += j - i;
        private void CheckOpenSubKeyPermission(string subkeyName, bool subKeyWritable)
            // If the parent key is not opened under default mode, we have access already.
            // If the parent key is opened under default mode, we need to check for permission.
            if (_checkMode == RegistryKeyPermissionCheck.Default)
                CheckSubKeyReadPermission(subkeyName);
            if (subKeyWritable && (_checkMode == RegistryKeyPermissionCheck.ReadSubTree))
                CheckSubTreeReadWritePermission(subkeyName);
        private void CheckOpenSubKeyPermission(string subkeyName, RegistryKeyPermissionCheck subKeyCheck)
            if (subKeyCheck == RegistryKeyPermissionCheck.Default)
            CheckSubTreePermission(subkeyName, subKeyCheck);
        private void CheckSubTreePermission(string subkeyName, RegistryKeyPermissionCheck subKeyCheck)
            if (subKeyCheck == RegistryKeyPermissionCheck.ReadSubTree)
                    CheckSubTreeReadPermission(subkeyName);
            else if (subKeyCheck == RegistryKeyPermissionCheck.ReadWriteSubTree)
                if (_checkMode != RegistryKeyPermissionCheck.ReadWriteSubTree)
        // Suppressed because keyName and subkeyName won't change.
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity")]
        private void CheckSubKeyWritePermission(string subkeyName)
            BCLDebug.Assert(_checkMode != RegistryKeyPermissionCheck.ReadSubTree, "We shouldn't allow creating sub key under read-only key!");
                // If we want to open a subkey of a read-only key as writeable, we need to do the check.
                new RegistryPermission(RegistryPermissionAccess.Write, _keyName + "\\" + subkeyName + "\\.").Demand();
        private void CheckSubKeyReadPermission(string subkeyName)
            BCLDebug.Assert(_checkMode == RegistryKeyPermissionCheck.Default, "Should be called from a key opened under default mode only!");
            new RegistryPermission(RegistryPermissionAccess.Read, _keyName + "\\" + subkeyName + "\\.").Demand();
        private void CheckSubKeyCreatePermission(string subkeyName)
                new RegistryPermission(RegistryPermissionAccess.Create, _keyName + "\\" + subkeyName + "\\.").Demand();
        private void CheckSubTreeReadPermission(string subkeyName)
                new RegistryPermission(RegistryPermissionAccess.Read, _keyName + "\\" + subkeyName + "\\").Demand();
        private void CheckSubTreeWritePermission(string subkeyName)
            BCLDebug.Assert(_checkMode != RegistryKeyPermissionCheck.ReadSubTree, "We shouldn't allow writing value to read-only key!");
                new RegistryPermission(RegistryPermissionAccess.Write, _keyName + "\\" + subkeyName + "\\").Demand();
        // Suppressed because keyName and valueName won't change.
        private void CheckSubTreeReadWritePermission(string subkeyName)
            new RegistryPermission(RegistryPermissionAccess.Write | RegistryPermissionAccess.Read,
                    _keyName + "\\" + subkeyName).Demand();
        private void CheckValueWritePermission(string valueName)
            // skip the security check if the key is opened under write mode
                new RegistryPermission(RegistryPermissionAccess.Write, _keyName + "\\" + valueName).Demand();
        private void CheckValueCreatePermission(string valueName)
            BCLDebug.Assert(_checkMode != RegistryKeyPermissionCheck.ReadSubTree, "We shouldn't allow creating value under read-only key!");
                new RegistryPermission(RegistryPermissionAccess.Create, _keyName + "\\" + valueName).Demand();
        private void CheckValueReadPermission(string valueName)
                // only need to check for default mode (dynamic check)
                new RegistryPermission(RegistryPermissionAccess.Read, _keyName + "\\" + valueName).Demand();
        // Suppressed because keyName won't change.
        private void CheckKeyReadPermission()
                new RegistryPermission(RegistryPermissionAccess.Read, _keyName + "\\.").Demand();
        private bool ContainsRegistryValue(string name)
            int retval = Win32Native.RegQueryValueEx(_hkey, name, null, ref type, (byte[])null, ref datasize);
            return retval == 0;
        private void EnsureNotDisposed()
            if (_hkey == null)
                throw new ObjectDisposedException(_keyName,
                                  RegistryProviderStrings.ObjectDisposed_RegKeyClosed);
        private void EnsureWriteable()
            if (!IsWritable())
                throw new UnauthorizedAccessException(RegistryProviderStrings.UnauthorizedAccess_RegistryNoWrite);
        private static int GetRegistryKeyAccess(bool isWritable)
            int winAccess;
            if (!isWritable)
                winAccess = Win32Native.KEY_READ;
                winAccess = Win32Native.KEY_READ | Win32Native.KEY_WRITE;
            return winAccess;
        private static int GetRegistryKeyAccess(RegistryKeyPermissionCheck mode)
            int winAccess = 0;
                case RegistryKeyPermissionCheck.ReadSubTree:
                case RegistryKeyPermissionCheck.Default:
                case RegistryKeyPermissionCheck.ReadWriteSubTree:
                    BCLDebug.Assert(false, "unexpected code path");
        private RegistryKeyPermissionCheck GetSubKeyPermissionCheck(bool subkeyWritable)
                return _checkMode;
            if (subkeyWritable)
                return RegistryKeyPermissionCheck.ReadWriteSubTree;
                return RegistryKeyPermissionCheck.ReadSubTree;
        private static void ValidateKeyName(string name)
            ArgumentNullException.ThrowIfNull(name, RegistryProviderStrings.Arg_Name);
            int nextSlash = name.IndexOf('\\');
            int current = 0;
            while (nextSlash != -1)
                if ((nextSlash - current) > MaxKeyLength)
                    throw new ArgumentException(RegistryProviderStrings.Arg_RegKeyStrLenBug);
                current = nextSlash + 1;
                nextSlash = name.IndexOf('\\', current);
            if ((name.Length - current) > MaxKeyLength)
        private static void ValidateKeyMode(RegistryKeyPermissionCheck mode)
            if (mode < RegistryKeyPermissionCheck.Default || mode > RegistryKeyPermissionCheck.ReadWriteSubTree)
                throw new ArgumentException(RegistryProviderStrings.Argument_InvalidRegistryKeyPermissionCheck);
        private static void ValidateKeyRights(int rights)
            if (0 != (rights & ~((int)RegistryRights.FullControl)))
        private void VerifyTransaction()
            if (_myTransaction == null)
                throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_NotAssociatedWithTransaction);
        // Win32 constants for error handling

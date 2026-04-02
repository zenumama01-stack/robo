 * The registry wrapper provides a common interface to both the transacted
 * and non-transacted registry APIs.  It is used exclusively by the registry provider
 * to perform registry operations.  In most cases, the wrapper simply forwards the
 * call to the appropriate registry API.
    internal interface IRegistryWrapper
        void SetValue(string? name, object value);
        void SetValue(string? name, object value, RegistryValueKind valueKind);
        string[] GetValueNames();
        void DeleteValue(string name);
        string[] GetSubKeyNames();
        IRegistryWrapper? CreateSubKey(string subkey);
        IRegistryWrapper? OpenSubKey(string name, bool writable);
        void DeleteSubKeyTree(string subkey);
        object? GetValue(string? name);
        object? GetValue(string? name, object? defaultValue, RegistryValueOptions options);
        RegistryValueKind GetValueKind(string? name);
        object RegistryKey { get; }
        void SetAccessControl(ObjectSecurity securityDescriptor);
        ObjectSecurity GetAccessControl(AccessControlSections includeSections);
        int SubKeyCount { get; }
    internal static class RegistryWrapperUtils
        public static object ConvertValueToUIntFromRegistryIfNeeded(string name, object value, RegistryValueKind kind)
                // Workaround for CLR bug that doesn't support full range of DWORD or QWORD
                if (kind == RegistryValueKind.DWord)
                    value = (int)value;
                    if ((int)value < 0)
                        value = BitConverter.ToUInt32(BitConverter.GetBytes((int)value), 0);
                else if (kind == RegistryValueKind.QWord)
                    value = (long)value;
                    if ((long)value < 0)
                        value = BitConverter.ToUInt64(BitConverter.GetBytes((long)value), 0);
                // This is expected if the value does not exist.
        public static object ConvertUIntToValueForRegistryIfNeeded(object value, RegistryValueKind kind)
                UInt32 intValue = 0;
                // See if it's already a positive number
                    intValue = Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                    value = BitConverter.ToInt32(BitConverter.GetBytes(intValue), 0);
                    // It must be a negative Int32, and therefore need no more conversion
                UInt64 intValue = 0;
                    intValue = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                    value = BitConverter.ToInt64(BitConverter.GetBytes(intValue), 0);
                    // It must be a negative Int64, and therefore need no more conversion
    internal class RegistryWrapper : IRegistryWrapper
        private readonly RegistryKey _regKey;
        internal RegistryWrapper(RegistryKey regKey)
            _regKey = regKey;
        #region IRegistryWrapper Members
            _regKey.SetValue(name, value);
            value = System.Management.Automation.PSObject.Base(value);
            value = RegistryWrapperUtils.ConvertUIntToValueForRegistryIfNeeded(value, valueKind);
            _regKey.SetValue(name, value, valueKind);
            return _regKey.GetValueNames();
            _regKey.DeleteValue(name);
            return _regKey.GetSubKeyNames();
        public IRegistryWrapper CreateSubKey(string subkey)
            RegistryKey newKey = _regKey.CreateSubKey(subkey);
            if (newKey == null)
                return new RegistryWrapper(newKey);
        public IRegistryWrapper OpenSubKey(string name, bool writable)
            RegistryKey newKey = _regKey.OpenSubKey(name, writable);
            _regKey.DeleteSubKeyTree(subkey);
            object value = _regKey.GetValue(name);
                value = RegistryWrapperUtils.ConvertValueToUIntFromRegistryIfNeeded(name, value, GetValueKind(name));
            object value = _regKey.GetValue(name, defaultValue, options);
            return _regKey.GetValueKind(name);
            _regKey.Dispose();
            get { return _regKey.Name; }
        public int SubKeyCount
            get { return _regKey.SubKeyCount; }
        public object RegistryKey
            get { return _regKey; }
            _regKey.SetAccessControl((RegistrySecurity)securityDescriptor);
            return _regKey.GetAccessControl(includeSections);
    internal class TransactedRegistryWrapper : IRegistryWrapper
        private readonly TransactedRegistryKey _txRegKey;
        internal TransactedRegistryWrapper(TransactedRegistryKey txRegKey, CmdletProvider provider)
            _txRegKey = txRegKey;
            using (_provider.CurrentPSTransaction)
                _txRegKey.SetValue(name, value);
                _txRegKey.SetValue(name, value, valueKind);
                return _txRegKey.GetValueNames();
                _txRegKey.DeleteValue(name);
                return _txRegKey.GetSubKeyNames();
                TransactedRegistryKey newKey = _txRegKey.CreateSubKey(subkey);
                    return new TransactedRegistryWrapper(newKey, _provider);
                TransactedRegistryKey newKey = _txRegKey.OpenSubKey(name, writable);
                _txRegKey.DeleteSubKeyTree(subkey);
                object value = _txRegKey.GetValue(name);
                object value = _txRegKey.GetValue(name, defaultValue, options);
                return _txRegKey.GetValueKind(name);
                _txRegKey.Close();
                    return _txRegKey.Name;
                    return _txRegKey.SubKeyCount;
            get { return _txRegKey; }
                _txRegKey.SetAccessControl((TransactedRegistrySecurity)securityDescriptor);
                return _txRegKey.GetAccessControl(includeSections);

** native CoTaskMem allocated via SecureStringToCoTaskMemUnicode.
    internal sealed class CoTaskMemUnicodeSafeHandle : SafeHandle
        internal CoTaskMemUnicodeSafeHandle()
        internal CoTaskMemUnicodeSafeHandle(IntPtr handle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
            Marshal.ZeroFreeCoTaskMemUnicode(handle);
        // DONT compare CoTaskMemUnicodeSafeHandle with CoTaskMemUnicodeSafeHandle.Zero
        public static CoTaskMemUnicodeSafeHandle Zero
                return new CoTaskMemUnicodeSafeHandle();

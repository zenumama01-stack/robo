    /// Represents the OLE struct PROPVARIANT.
    /// This class is intended for internal use only.
    /// Originally sourced from https://blogs.msdn.com/adamroot/pages/interop-with-propvariants-in-net.aspx
    /// and modified to add ability to set values
    internal sealed class PropVariant : IDisposable
        // This is actually a VarEnum value, but the VarEnum type requires 4 bytes instead of the expected 2.
        private readonly ushort _valueType;
        [FieldOffset(8)]
        private readonly IntPtr _ptr;
        /// Set a string value.
        internal PropVariant(string value)
                throw new ArgumentException("PropVariantNullString", nameof(value));
            _valueType = (ushort)VarEnum.VT_LPWSTR;
            _ptr = Marshal.StringToCoTaskMemUni(value);
        /// Disposes the object, calls the clear function.
            PropVariantNativeMethods.PropVariantClear(this);
        /// Finalizes an instance of the <see cref="PropVariant"/> class.
        ~PropVariant()
        private static class PropVariantNativeMethods
            [DllImport("Ole32.dll", PreserveSig = false)]
            internal static extern void PropVariantClear([In, Out] PropVariant pvar);

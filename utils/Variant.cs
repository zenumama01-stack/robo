    /// Variant is the basic COM type for late-binding. It can contain any other COM data type.
    /// This type definition precisely matches the unmanaged data layout so that the struct can be passed
    /// to and from COM calls.
        static Variant()
            // Variant size is the size of 4 pointers (16 bytes) on a 32-bit processor,
            // and 3 pointers (24 bytes) on a 64-bit processor.
            int variantSize = Marshal.SizeOf(typeof(Variant));
                Debug.Assert(variantSize == (4 * IntPtr.Size));
                Debug.Assert(IntPtr.Size == 8);
                Debug.Assert(variantSize == (3 * IntPtr.Size));
        [FieldOffset(0)] private TypeUnion _typeUnion;
        [FieldOffset(0)] private decimal _decimal;
        private struct TypeUnion
            public ushort _vt;
            public ushort _wReserved1;
            public ushort _wReserved2;
            public ushort _wReserved3;
            public UnionTypes _unionTypes;
        private struct Record
            public IntPtr _record;
            public IntPtr _recordInfo;
        private struct UnionTypes
            [FieldOffset(0)] public sbyte _i1;
            [FieldOffset(0)] public short _i2;
            [FieldOffset(0)] public int _i4;
            [FieldOffset(0)] public long _i8;
            [FieldOffset(0)] public byte _ui1;
            [FieldOffset(0)] public ushort _ui2;
            [FieldOffset(0)] public uint _ui4;
            [FieldOffset(0)] public ulong _ui8;
            [FieldOffset(0)] public int _int;
            [FieldOffset(0)] public uint _uint;
            [FieldOffset(0)] public short _bool;
            [FieldOffset(0)] public int _error;
            [FieldOffset(0)] public float _r4;
            [FieldOffset(0)] public double _r8;
            [FieldOffset(0)] public long _cy;
            [FieldOffset(0)] public double _date;
            [FieldOffset(0)] public IntPtr _bstr;
            [FieldOffset(0)] public IntPtr _unknown;
            [FieldOffset(0)] public IntPtr _dispatch;
            [FieldOffset(0)] public IntPtr _pvarVal;
            [FieldOffset(0)] public IntPtr _byref;
            [FieldOffset(0)] public Record _record;
        /// Primitive types are the basic COM types. It includes valuetypes like ints, but also reference types
        /// like BStrs. It does not include composite types like arrays and user-defined COM types (IUnknown/IDispatch).
        public static bool IsPrimitiveType(VarEnum varEnum)
        public unsafe void CopyFromIndirect(object value)
            VarEnum vt = (VarEnum)(((int)this.VariantType) & ~((int)VarEnum.VT_BYREF));
                if (vt == VarEnum.VT_DISPATCH || vt == VarEnum.VT_UNKNOWN || vt == VarEnum.VT_BSTR)
                    *(IntPtr*)this._typeUnion._unionTypes._byref = IntPtr.Zero;
            if ((vt & VarEnum.VT_ARRAY) != 0)
                Variant vArray;
                Marshal.GetNativeVariantForObject(value, (IntPtr)(void*)&vArray);
                *(IntPtr*)this._typeUnion._unionTypes._byref = vArray._typeUnion._unionTypes._byref;
                    *(sbyte*)this._typeUnion._unionTypes._byref = (sbyte)value;
                    *(byte*)this._typeUnion._unionTypes._byref = (byte)value;
                    *(short*)this._typeUnion._unionTypes._byref = (short)value;
                    *(ushort*)this._typeUnion._unionTypes._byref = (ushort)value;
                    // VARIANT_TRUE  = -1
                    // VARIANT_FALSE = 0
                    *(short*)this._typeUnion._unionTypes._byref = (bool)value ? (short)-1 : (short)0;
                    *(int*)this._typeUnion._unionTypes._byref = (int)value;
                    *(uint*)this._typeUnion._unionTypes._byref = (uint)value;
                    *(int*)this._typeUnion._unionTypes._byref = ((ErrorWrapper)value).ErrorCode;
                    *(long*)this._typeUnion._unionTypes._byref = (long)value;
                    *(ulong*)this._typeUnion._unionTypes._byref = (ulong)value;
                    *(float*)this._typeUnion._unionTypes._byref = (float)value;
                    *(double*)this._typeUnion._unionTypes._byref = (double)value;
                    *(double*)this._typeUnion._unionTypes._byref = ((DateTime)value).ToOADate();
                    *(IntPtr*)this._typeUnion._unionTypes._byref = Marshal.GetIUnknownForObject(value);
                    *(IntPtr*)this._typeUnion._unionTypes._byref = Marshal.GetComInterfaceForObject<object, IDispatch>(value);
                    *(IntPtr*)this._typeUnion._unionTypes._byref = Marshal.StringToBSTR((string)value);
                    *(long*)this._typeUnion._unionTypes._byref = decimal.ToOACurrency((decimal)value);
                    *(decimal*)this._typeUnion._unionTypes._byref = (decimal)value;
                    Marshal.GetNativeVariantForObject(value, this._typeUnion._unionTypes._byref);
                    throw new ArgumentException();
        /// Get the managed object representing the Variant.
        public object? ToObject()
            // Check the simple case upfront
            if (IsEmpty)
            switch (VariantType)
                    return DBNull.Value;
                case VarEnum.VT_I1: return AsI1;
                case VarEnum.VT_I2: return AsI2;
                case VarEnum.VT_I4: return AsI4;
                case VarEnum.VT_I8: return AsI8;
                case VarEnum.VT_UI1: return AsUi1;
                case VarEnum.VT_UI2: return AsUi2;
                case VarEnum.VT_UI4: return AsUi4;
                case VarEnum.VT_UI8: return AsUi8;
                case VarEnum.VT_INT: return AsInt;
                case VarEnum.VT_UINT: return AsUint;
                case VarEnum.VT_BOOL: return AsBool;
                case VarEnum.VT_ERROR: return AsError;
                case VarEnum.VT_R4: return AsR4;
                case VarEnum.VT_R8: return AsR8;
                case VarEnum.VT_DECIMAL: return AsDecimal;
                case VarEnum.VT_CY: return AsCy;
                case VarEnum.VT_DATE: return AsDate;
                case VarEnum.VT_BSTR: return AsBstr;
                case VarEnum.VT_UNKNOWN: return AsUnknown;
                case VarEnum.VT_DISPATCH: return AsDispatch;
                        fixed (void* pThis = &this)
                            return Marshal.GetObjectForNativeVariant((System.IntPtr)pThis);
        /// Release any unmanaged memory associated with the Variant
            // We do not need to call OLE32's VariantClear for primitive types or ByRefs
            // to save ourselves the cost of interop transition.
            // ByRef indicates the memory is not owned by the VARIANT itself while
            // primitive types do not have any resources to free up.
            // Hence, only safearrays, BSTRs, interfaces and user types are
            // handled differently.
            VarEnum vt = VariantType;
            if ((vt & VarEnum.VT_BYREF) != 0)
                VariantType = VarEnum.VT_EMPTY;
            else if (((vt & VarEnum.VT_ARRAY) != 0)
                    || (vt == VarEnum.VT_BSTR)
                    || (vt == VarEnum.VT_UNKNOWN)
                    || (vt == VarEnum.VT_DISPATCH)
                    || (vt == VarEnum.VT_VARIANT)
                    || (vt == VarEnum.VT_RECORD))
                        Interop.Windows.VariantClear((nint)pThis);
                Debug.Assert(IsEmpty);
        public VarEnum VariantType
            get => (VarEnum)_typeUnion._vt;
            set => _typeUnion._vt = (ushort)value;
        public bool IsEmpty => _typeUnion._vt == ((ushort)VarEnum.VT_EMPTY);
        public bool IsByRef => (_typeUnion._vt & ((ushort)VarEnum.VT_BYREF)) != 0;
        public void SetAsNULL()
            VariantType = VarEnum.VT_NULL;
        public sbyte AsI1
                Debug.Assert(VariantType == VarEnum.VT_I1);
                return _typeUnion._unionTypes._i1;
                VariantType = VarEnum.VT_I1;
                _typeUnion._unionTypes._i1 = value;
        public short AsI2
                Debug.Assert(VariantType == VarEnum.VT_I2);
                return _typeUnion._unionTypes._i2;
                VariantType = VarEnum.VT_I2;
                _typeUnion._unionTypes._i2 = value;
        public int AsI4
                Debug.Assert(VariantType == VarEnum.VT_I4);
                return _typeUnion._unionTypes._i4;
                VariantType = VarEnum.VT_I4;
                _typeUnion._unionTypes._i4 = value;
        public long AsI8
                Debug.Assert(VariantType == VarEnum.VT_I8);
                return _typeUnion._unionTypes._i8;
                VariantType = VarEnum.VT_I8;
                _typeUnion._unionTypes._i8 = value;
        public byte AsUi1
                Debug.Assert(VariantType == VarEnum.VT_UI1);
                return _typeUnion._unionTypes._ui1;
                VariantType = VarEnum.VT_UI1;
                _typeUnion._unionTypes._ui1 = value;
        public ushort AsUi2
                Debug.Assert(VariantType == VarEnum.VT_UI2);
                return _typeUnion._unionTypes._ui2;
                VariantType = VarEnum.VT_UI2;
                _typeUnion._unionTypes._ui2 = value;
        public uint AsUi4
                Debug.Assert(VariantType == VarEnum.VT_UI4);
                return _typeUnion._unionTypes._ui4;
                VariantType = VarEnum.VT_UI4;
                _typeUnion._unionTypes._ui4 = value;
        public ulong AsUi8
                Debug.Assert(VariantType == VarEnum.VT_UI8);
                return _typeUnion._unionTypes._ui8;
                VariantType = VarEnum.VT_UI8;
                _typeUnion._unionTypes._ui8 = value;
        public int AsInt
                Debug.Assert(VariantType == VarEnum.VT_INT);
                return _typeUnion._unionTypes._int;
                VariantType = VarEnum.VT_INT;
                _typeUnion._unionTypes._int = value;
        public uint AsUint
                Debug.Assert(VariantType == VarEnum.VT_UINT);
                return _typeUnion._unionTypes._uint;
                VariantType = VarEnum.VT_UINT;
                _typeUnion._unionTypes._uint = value;
        public bool AsBool
                Debug.Assert(VariantType == VarEnum.VT_BOOL);
                return _typeUnion._unionTypes._bool != 0;
                VariantType = VarEnum.VT_BOOL;
                _typeUnion._unionTypes._bool = value ? (short)-1 : (short)0;
        public int AsError
                Debug.Assert(VariantType == VarEnum.VT_ERROR);
                return _typeUnion._unionTypes._error;
                VariantType = VarEnum.VT_ERROR;
                _typeUnion._unionTypes._error = value;
        public float AsR4
                Debug.Assert(VariantType == VarEnum.VT_R4);
                return _typeUnion._unionTypes._r4;
                VariantType = VarEnum.VT_R4;
                _typeUnion._unionTypes._r4 = value;
        public double AsR8
                Debug.Assert(VariantType == VarEnum.VT_R8);
                return _typeUnion._unionTypes._r8;
                VariantType = VarEnum.VT_R8;
                _typeUnion._unionTypes._r8 = value;
        public decimal AsDecimal
                Debug.Assert(VariantType == VarEnum.VT_DECIMAL);
                // The first byte of Decimal is unused, but usually set to 0
                Variant v = this;
                v._typeUnion._vt = 0;
                return v._decimal;
                VariantType = VarEnum.VT_DECIMAL;
                _decimal = value;
                // _vt overlaps with _decimal, and should be set after setting _decimal
                _typeUnion._vt = (ushort)VarEnum.VT_DECIMAL;
        public decimal AsCy
                Debug.Assert(VariantType == VarEnum.VT_CY);
                return decimal.FromOACurrency(_typeUnion._unionTypes._cy);
                VariantType = VarEnum.VT_CY;
                _typeUnion._unionTypes._cy = decimal.ToOACurrency(value);
        public DateTime AsDate
                Debug.Assert(VariantType == VarEnum.VT_DATE);
                return DateTime.FromOADate(_typeUnion._unionTypes._date);
                VariantType = VarEnum.VT_DATE;
                _typeUnion._unionTypes._date = value.ToOADate();
        public string AsBstr
                Debug.Assert(VariantType == VarEnum.VT_BSTR);
                return (string)Marshal.PtrToStringBSTR(this._typeUnion._unionTypes._bstr);
                VariantType = VarEnum.VT_BSTR;
                this._typeUnion._unionTypes._bstr = Marshal.StringToBSTR(value);
        public object? AsUnknown
                Debug.Assert(VariantType == VarEnum.VT_UNKNOWN);
                if (_typeUnion._unionTypes._unknown == IntPtr.Zero)
                return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._unknown);
                VariantType = VarEnum.VT_UNKNOWN;
                    _typeUnion._unionTypes._unknown = IntPtr.Zero;
                    _typeUnion._unionTypes._unknown = Marshal.GetIUnknownForObject(value);
        public object? AsDispatch
                Debug.Assert(VariantType == VarEnum.VT_DISPATCH);
                if (_typeUnion._unionTypes._dispatch == IntPtr.Zero)
                return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._dispatch);
                VariantType = VarEnum.VT_DISPATCH;
                    _typeUnion._unionTypes._dispatch = IntPtr.Zero;
                    _typeUnion._unionTypes._dispatch = Marshal.GetComInterfaceForObject<object, IDispatch>(value);
        public IntPtr AsByRefVariant
                Debug.Assert(VariantType == (VarEnum.VT_BYREF | VarEnum.VT_VARIANT));
                return _typeUnion._unionTypes._pvarVal;

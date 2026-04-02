using ComTypes = System.Runtime.InteropServices.ComTypes;
    [Guid("00020400-0000-0000-C000-000000000046")]
        int TryGetTypeInfoCount(out uint pctinfo);
        int TryGetTypeInfo(uint iTInfo, int lcid, out IntPtr info);
        int TryGetIDsOfNames(
            ref Guid iid,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)]
            string[] names,
            uint cNames,
            [Out]
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeParamIndex = 2)]
            int[] rgDispId);
        int TryInvoke(
            ComTypes.INVOKEKIND wFlags,
            ref ComTypes.DISPPARAMS pDispParams,
            IntPtr VarResult,
            IntPtr pExcepInfo,
            IntPtr puArgErr);
    [Guid("B196B283-BAB4-101A-B69C-00AA00341D07")]
    internal interface IProvideClassInfo
        void GetClassInfo(out IntPtr info);
    internal static class ComDispIds
        internal const int DISPID_VALUE = 0;
        internal const int DISPID_PROPERTYPUT = -3;

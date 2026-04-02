using COM = System.Runtime.InteropServices.ComTypes;
    /// The IDispatch interface.
    [Guid("00020400-0000-0000-c000-000000000046")]
    internal interface IDispatch
        int GetTypeInfoCount(out int info);
        int GetTypeInfo(int iTInfo, int lcid, out COM.ITypeInfo? ppTInfo);
        void GetIDsOfNames(
            [MarshalAs(UnmanagedType.LPStruct)] Guid iid,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames,
            int cNames,
            int lcid,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] rgDispId);
        void Invoke(
            int dispIdMember,
            COM.INVOKEKIND wFlags,
            [In, Out][MarshalAs(UnmanagedType.LPArray)] COM.DISPPARAMS[] paramArray,
            out object? pVarResult,
            out ComInvoker.EXCEPINFO pExcepInfo,
            out uint puArgErr);

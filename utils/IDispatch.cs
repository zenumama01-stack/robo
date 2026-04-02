        int GetTypeInfoCount();
        ComTypes.ITypeInfo GetTypeInfo(
            int iTInfo,
            int lcid);
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2), In]
            string[] rgszNames,
            [Out] int[] rgDispId);
        // The last 3 parameters of Invoke() are optional and must be defined
        // as IntPtr in C#, since there is no language feature for optional ref/out.
            /* out/optional */ IntPtr pVarResult,
            /* out/optional */ IntPtr pExcepInfo,
            /* out/optional */ IntPtr puArgErr);
    internal enum InvokeFlags : short
        DISPATCH_METHOD = 1,
        DISPATCH_PROPERTYGET = 2,
        DISPATCH_PROPERTYPUT = 4,
        DISPATCH_PROPERTYPUTREF = 8

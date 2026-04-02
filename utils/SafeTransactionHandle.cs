    [Guid("79427A2B-F895-40e0-BE79-B57DC82ED231"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IKernelTransaction
        int GetHandle(out IntPtr pHandle);
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal sealed class SafeTransactionHandle : SafeHandleZeroOrMinusOneIsInvalid
        private const string resBaseName = "RegistryProviderStrings";
        private SafeTransactionHandle(IntPtr handle)
            this.handle = handle;
        internal static SafeTransactionHandle Create()
            return SafeTransactionHandle.Create(Transaction.Current);
        internal static SafeTransactionHandle Create(Transaction managedTransaction)
            if (managedTransaction == null)
                throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_NeedTransaction);
            // MSDTC is not available on WinPE machine.
            // CommitableTransaction will use DTC APIs under the covers to get KTM transaction manager interface.
            // KTM is kernel Transaction Manager to handle file, registry etc and MSDTC provides an integration support
            // with KTM to handle transaction across kernel resources and MSDTC resources like SQL, MSMQ etc.
            // We need KTMRM service as well. WinPE doesn't have these services installed
            if (Utils.IsWinPEHost() || PsUtils.IsRunningOnProcessorArchitectureARM())
                throw new NotSupportedException(RegistryProviderStrings.NotSupported_KernelTransactions);
            IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(managedTransaction);
            IKernelTransaction ktmInterface = dtcTransaction as IKernelTransaction;
            if (ktmInterface == null)
            IntPtr ktmTxHandle;
            int hr = ktmInterface.GetHandle(out ktmTxHandle);
            HandleError(hr);
            return new SafeTransactionHandle(ktmTxHandle);
            // We don't care about the value of GetLastError.
#pragma warning suppress 56523
            return Win32Native.CloseHandle(this.handle);
        private static void HandleError(int error)
            if (error != Win32Native.ERROR_SUCCESS)

        internal const int COINIT_APARTMENTTHREADED = 0x2;
        internal const int E_NOTIMPL = unchecked((int)0X80004001);
        [LibraryImport("api-ms-win-core-com-l1-1-0.dll")]
        internal static partial int CoInitializeEx(nint reserve, int coinit);

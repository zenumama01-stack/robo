    /// HRESULT Wrapper
    internal enum HResult
        /// S_OK
        Ok = 0x0000,
        /// S_FALSE.
        False = 0x0001,
        /// E_INVALIDARG.
        InvalidArguments = unchecked((int)0x80070057),
        /// E_OUTOFMEMORY.
        OutOfMemory = unchecked((int)0x8007000E),
        /// E_NOINTERFACE.
        NoInterface = unchecked((int)0x80004002),
        /// E_FAIL.
        Fail = unchecked((int)0x80004005),
        /// E_ELEMENTNOTFOUND.
        ElementNotFound = unchecked((int)0x80070490),
        /// TYPE_E_ELEMENTNOTFOUND.
        TypeElementNotFound = unchecked((int)0x8002802B),
        /// NO_OBJECT.
        NoObject = unchecked((int)0x800401E5),
        /// Win32 Error code: ERROR_CANCELLED.
        Win32ErrorCanceled = 1223,
        /// ERROR_CANCELLED.
        Canceled = unchecked((int)0x800704C7),
        /// The requested resource is in use.
        ResourceInUse = unchecked((int)0x800700AA),
        /// The requested resources is read-only.
        AccessDenied = unchecked((int)0x80030005)

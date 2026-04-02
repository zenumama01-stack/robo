    ///    Strongly-typed and parameterized exception factory.
    internal static partial class Error
        /// InvalidOperationException with message like "Marshal.SetComObjectData failed."
        internal static Exception SetComObjectDataFailed()
            return new InvalidOperationException(ParserStrings.SetComObjectDataFailed);
        /// InvalidOperationException with message like "Unexpected VarEnum {0}."
        internal static Exception UnexpectedVarEnum(object p0)
            return new InvalidOperationException(StringUtil.Format(ParserStrings.UnexpectedVarEnum, p0));
        /// System.Reflection.TargetParameterCountException with message like "Error while invoking {0}."
        internal static Exception DispBadParamCount(object p0, int parameterCount)
            return new System.Reflection.TargetParameterCountException(StringUtil.Format(ParserStrings.DispBadParamCount, p0, parameterCount));
        /// MissingMemberException with message like "Error while invoking {0}."
        internal static Exception DispMemberNotFound(object p0)
            return new MissingMemberException(StringUtil.Format(ParserStrings.DispMemberNotFound, p0));
        /// ArgumentException with message like "Error while invoking {0}. Named arguments are not supported."
        internal static Exception DispNoNamedArgs(object p0)
            return new ArgumentException(StringUtil.Format(ParserStrings.DispNoNamedArgs, p0));
        /// OverflowException with message like "Error while invoking {0}."
        internal static Exception DispOverflow(object p0)
            return new OverflowException(StringUtil.Format(ParserStrings.DispOverflow, p0));
        /// ArgumentException with message like "Could not convert argument {0} for call to {1}."
        internal static Exception DispTypeMismatch(object method, string value, string originalTypeName, string destinationTypeName)
            return new ArgumentException(StringUtil.Format(ParserStrings.DispTypeMismatch, method, value, originalTypeName, destinationTypeName));
        /// ArgumentException with message like "Error while invoking {0}. A required parameter was omitted."
        internal static Exception DispParamNotOptional(object p0)
            return new ArgumentException(StringUtil.Format(ParserStrings.DispParamNotOptional, p0));
        /// InvalidOperationException with message like "Cannot retrieve type information."
        internal static Exception CannotRetrieveTypeInformation()
            return new InvalidOperationException(ParserStrings.CannotRetrieveTypeInformation);
        /// ArgumentException with message like "IDispatch::GetIDsOfNames behaved unexpectedly for {0}."
        internal static Exception GetIDsOfNamesInvalid(object p0)
            return new ArgumentException(StringUtil.Format(ParserStrings.GetIDsOfNamesInvalid, p0));
        /// InvalidOperationException with message like "Attempting to pass an event handler of an unsupported type."
        internal static Exception UnsupportedHandlerType()
            return new InvalidOperationException(ParserStrings.UnsupportedHandlerType);
        /// MissingMemberException with message like "Could not get dispatch ID for {0} (error: {1})."
        internal static Exception CouldNotGetDispId(object p0, object p1)
            return new MissingMemberException(StringUtil.Format(ParserStrings.CouldNotGetDispId, p0, p1));
        /// System.Reflection.AmbiguousMatchException with message like "There are valid conversions from {0} to {1}."
        internal static Exception AmbiguousConversion(object p0, object p1)
            return new System.Reflection.AmbiguousMatchException(StringUtil.Format(ParserStrings.AmbiguousConversion, p0, p1));
        // List of error constants https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes
        internal const int ERROR_SUCCESS = 0;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_GEN_FAILURE = 31;
        internal const int ERROR_NOT_SUPPORTED = 50;
        internal const int ERROR_NO_NETWORK = 1222;
        internal const int ERROR_MORE_DATA = 234;
        internal const int ERROR_CONNECTION_UNAVAIL = 1201;

        internal const sbyte PHCM_APPLICATION_DEFAULT = 0;
        internal const sbyte PHCM_DISGUISE_PLACEHOLDER = 1;
        internal const sbyte PHCM_EXPOSE_PLACEHOLDERS = 2;
        internal const sbyte PHCM_MAX = 2;
        internal const sbyte PHCM_ERROR_INVALID_PARAMETER = -1;
        internal const sbyte PHCM_ERROR_NO_TEB = -2;
        internal static partial sbyte RtlQueryProcessPlaceholderCompatibilityMode();
        internal static partial sbyte RtlSetProcessPlaceholderCompatibilityMode(sbyte pcm);

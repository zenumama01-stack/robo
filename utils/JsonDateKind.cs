    /// Enums for ConvertFrom-Json -DateKind parameter.
    public enum JsonDateKind
        /// DateTime values are returned as a DateTime with the Kind representing the time zone in the raw string.
        /// DateTime values are returned as the Local kind representation of the value.
        Local,
        /// DateTime values are returned as the UTC kind representation of the value.
        Utc,
        /// DateTime values are returned as a DateTimeOffset value preserving the timezone information.
        Offset,
        /// DateTime values are returned as raw strings.

    /// A completer for HTTP version names.
    internal sealed class HttpVersionCompletionsAttribute : ArgumentCompletionsAttribute
        public static readonly string[] AllowedVersions;
        static HttpVersionCompletionsAttribute()
            FieldInfo[] fields = typeof(HttpVersion).GetFields(BindingFlags.Static | BindingFlags.Public);
            var versions = new List<string>(fields.Length - 1);
            for (int i = 0; i < fields.Length; i++)
                // skip field Unknown and not Version type
                if (fields[i].Name == nameof(HttpVersion.Unknown) || fields[i].FieldType != typeof(Version))
                var version = (Version?)fields[i].GetValue(null);
                if (version is not null)
                    versions.Add(version.ToString());
            AllowedVersions = versions.ToArray();
        public HttpVersionCompletionsAttribute() : base(AllowedVersions)

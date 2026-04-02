    /// This class represents a help system URI.
    internal class UpdatableHelpUri
        /// <param name="moduleGuid">Module guid.</param>
        /// <param name="culture">UI culture.</param>
        /// <param name="resolvedUri">Resolved URI.</param>
        internal UpdatableHelpUri(string moduleName, Guid moduleGuid, CultureInfo culture, string resolvedUri)
            Debug.Assert(!string.IsNullOrEmpty(resolvedUri));
            ModuleGuid = moduleGuid;
            ResolvedUri = resolvedUri;
        internal Guid ModuleGuid { get; }
        /// UI Culture.
        internal CultureInfo Culture { get; }
        /// Resolved URI.
        internal string ResolvedUri { get; }

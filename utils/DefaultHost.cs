using Dbg = System.Diagnostics;
    /// This is the default host implementing PSHost offering minimal host capabilities.
    /// Runspace is the primary user of this class.
    internal class DefaultHost : PSHost
        /// Creates an instance based on the current culture and current UI culture.
        /// <param name="currentCulture">Current culture for this host.</param>
        /// <param name="currentUICulture">Current UI culture for this host.</param>
        internal DefaultHost(CultureInfo currentCulture, CultureInfo currentUICulture)
            CurrentCulture = currentCulture;
            CurrentUICulture = currentUICulture;
        /// <summary>See base class</summary>
        public override string Name { get { return "Default Host"; } }
        public override Guid InstanceId { get; } = Guid.NewGuid();
        /// This property is not supported.
        public override PSHostUserInterface UI { get { return null; } }
        public override CultureInfo CurrentCulture { get; } = null;
        public override CultureInfo CurrentUICulture { get; } = null;
        SetShouldExit(int exitCode)
            // No op
        /// On calling this method
        EnterNestedPrompt()
        ExitNestedPrompt()
        NotifyBeginApplication()
        NotifyEndApplication()

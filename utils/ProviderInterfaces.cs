    /// Exposes the APIs to manage the Cmdlet Providers the Cmdlet base class. The methods of this class
    public sealed class CmdletProviderManagementIntrinsics
        private CmdletProviderManagementIntrinsics()
        /// The facade for managing providers.
        /// The session to which this is a facade.
        internal CmdletProviderManagementIntrinsics(SessionStateInternal sessionState)
        /// Gets the specified provider(s).
        /// Either the fully-qualified or friendly name for the provider.
        /// The provider information for the specified provider.
        /// If the provider specified by <paramref name="name"/> is not currently
        /// loaded.
        public Collection<ProviderInfo> Get(string name)
            return _sessionState.GetProvider(name);
        /// <exception cref="ProviderNameAmbiguousException">
        /// If <paramref name="name"/> is not PSSnapin-qualified and more than one provider
        /// exists with the specified name.
        public ProviderInfo GetOne(string name)
            return _sessionState.GetSingleProvider(name);
        /// Gets all the Cmdlet Providers that are loaded.
        public IEnumerable<ProviderInfo> GetAll()
            return _sessionState.ProviderList;
        #region Internal methods
        /// Determines if the specified provider has the specified capability.
        /// <param name="capability">
        /// The capability to check the provider for.
        /// The provider information to use for the check.
        /// True, if the provider has the capability, false otherwise.
        internal static bool CheckProviderCapabilities(
            ProviderCapabilities capability,
            ProviderInfo provider)
            // Check the capability
            return (provider.Capabilities & capability) != 0;
        /// Gets the count of the number of providers that are loaded.
                return _sessionState.ProviderCount;
        #endregion Internal methods

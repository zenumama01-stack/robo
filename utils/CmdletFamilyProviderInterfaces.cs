    /// Exposes the Cmdlet Family Providers to the Cmdlet base class. The methods of this class
    public sealed class ProviderIntrinsics
        private ProviderIntrinsics()
        /// An instance of the cmdlet.
        /// If <paramref name="cmdlet"/> is null.
        internal ProviderIntrinsics(Cmdlet cmdlet)
            Item = new ItemCmdletProviderIntrinsics(cmdlet);
            ChildItem = new ChildItemCmdletProviderIntrinsics(cmdlet);
            Content = new ContentCmdletProviderIntrinsics(cmdlet);
            Property = new PropertyCmdletProviderIntrinsics(cmdlet);
            SecurityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(cmdlet);
        internal ProviderIntrinsics(SessionStateInternal sessionState)
            Item = new ItemCmdletProviderIntrinsics(sessionState);
            ChildItem = new ChildItemCmdletProviderIntrinsics(sessionState);
            Content = new ContentCmdletProviderIntrinsics(sessionState);
            Property = new PropertyCmdletProviderIntrinsics(sessionState);
            SecurityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(sessionState);
        /// Gets the object that exposes the verbs for the item noun for Cmdlet Providers.
        public ItemCmdletProviderIntrinsics Item { get; }
        /// Gets the object that exposes the verbs for the childItem noun for Cmdlet Providers.
        public ChildItemCmdletProviderIntrinsics ChildItem { get; }
        /// Gets the object that exposes the verbs for the content noun for Cmdlet Providers.
        public ContentCmdletProviderIntrinsics Content { get; }
        /// Gets the object that exposes the verbs for the property noun for Cmdlet Providers.
        public PropertyCmdletProviderIntrinsics Property { get; }
        /// Gets the object that exposes the verbs for the SecurityDescriptor noun for Cmdlet Providers.
        public SecurityDescriptorCmdletProviderIntrinsics SecurityDescriptor { get; }
        private readonly InternalCommand _cmdlet;

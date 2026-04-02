    /// Declares a class as a Cmdlet provider.
    /// The class must be derived from System.Management.Automation.Provider.CmdletProvider to
    /// be recognized by the runspace.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CmdletProviderAttribute : Attribute
        /// Constructor for the attribute.
        /// The provider name.
        /// <param name="providerCapabilities">
        /// An enumeration of the capabilities that the provider implements beyond the
        /// default capabilities that are required.
        /// If <paramref name="providerName"/> contains any of the following characters: \ [ ] ? * :
        public CmdletProviderAttribute(
            ProviderCapabilities providerCapabilities)
                throw PSTraceSource.NewArgumentNullException(nameof(providerName));
            if (providerName.IndexOfAny(_illegalCharacters) != -1)
                    nameof(providerName),
                    SessionStateStrings.ProviderNameNotValid,
            ProviderName = providerName;
            ProviderCapabilities = providerCapabilities;
        private readonly char[] _illegalCharacters = new char[] { ':', '\\', '[', ']', '?', '*' };
        public string ProviderName { get; } = string.Empty;
        /// Gets the flags that represent the capabilities of the provider.
        public ProviderCapabilities ProviderCapabilities { get; } = ProviderCapabilities.None;
    /// This enumeration defines the capabilities that the provider implements.
    public enum ProviderCapabilities
        /// The provider does not add any additional capabilities beyond what the
        /// PowerShell engine provides.
        /// The provider does the inclusion filtering for those commands that take an Include
        /// parameter. The PowerShell engine should not try to do the filtering on behalf of this
        /// provider.
        /// The implementer of the provider should make every effort to filter in a way that is consistent
        /// with the PowerShell engine. This option is allowed because in many cases the provider
        /// can be much more efficient at filtering.
        Include = 0x1,
        /// The provider does the exclusion filtering for those commands that take an Exclude
        Exclude = 0x2,
        /// The provider can take a provider specific filter string.
        /// For implementers of providers using this attribute, a provider specific filter can be passed from
        /// the Core Commands to the provider. This filter string is not interpreted in any
        /// way by the PowerShell engine.
        Filter = 0x4,
        /// The provider does the wildcard matching for those commands that allow for it. The PowerShell
        /// engine should not try to do the wildcard matching on behalf of the provider when this
        /// flag is set.
        /// The implementer of the provider should make every effort to do the wildcard matching in a way that is consistent
        /// with the PowerShell engine. This option is allowed because in many cases wildcard matching
        /// cannot occur via the path name or because the provider can do the matching in a much more
        /// efficient manner.
        ExpandWildcards = 0x8,
        /// The provider supports ShouldProcess. When this capability is specified, the
        /// -Whatif and -Confirm parameters become available to the user when using
        ShouldProcess = 0x10,
        /// The provider supports credentials. When this capability is specified and
        /// the user passes credentials to the core cmdlets, those credentials will
        /// be passed to the provider. If the provider doesn't specify this capability
        /// and the user passes credentials, an exception is thrown.
        Credentials = 0x20,
        /// The provider supports transactions. When this capability is specified, PowerShell
        /// lets the provider participate in the current PowerShell transaction.
        /// The provider does not support this capability and the user attempts to apply a
        /// transaction to it, an exception is thrown.
        Transactions = 0x40,

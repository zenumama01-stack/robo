    /// Defines the names of the internal providers.
    /// Derived classes exist for custom and single shells. In the single
    /// shell the provider name includes the PSSnapin name. In custom
    /// shells it does not.
    internal abstract class ProviderNames
        /// Gets the name of the EnvironmentProvider.
        internal abstract string Environment { get; }
        /// Gets the name of the Certificate.
        internal abstract string Certificate { get; }
        /// Gets the name of the VariableProvider.
        internal abstract string Variable { get; }
        /// Gets the name of the AliasProvider.
        internal abstract string Alias { get; }
        /// Gets the name of the FunctionProvider.
        internal abstract string Function { get; }
        /// Gets the name of the FileSystemProvider.
        internal abstract string FileSystem { get; }
        /// Gets the name of the RegistryProvider.
        internal abstract string Registry { get; }
    /// The provider names for the single shell.
    internal class SingleShellProviderNames : ProviderNames
        internal override string Environment
                return "Microsoft.PowerShell.Core\\Environment";
        internal override string Certificate
                return "Microsoft.PowerShell.Security\\Certificate";
        internal override string Variable
                return "Microsoft.PowerShell.Core\\Variable";
        internal override string Alias
                return "Microsoft.PowerShell.Core\\Alias";
        internal override string Function
                return "Microsoft.PowerShell.Core\\Function";
        internal override string FileSystem
                return "Microsoft.PowerShell.Core\\FileSystem";
        internal override string Registry
                return "Microsoft.PowerShell.Core\\Registry";

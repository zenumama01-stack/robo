    /// The base class for the */property commands that also take
    public class PassThroughItemPropertyCommandBase : ItemPropertyCommandBase
        /// Determines if the property returned from the provider should
                return base.DoesProviderSupportShouldProcess(base.paths);

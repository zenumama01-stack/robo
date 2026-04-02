    /// Defines the base class for all of the classes the provide implementations for a particular
    /// data store or item for the PowerShell core commands.
    public abstract partial class CmdletProvider
        #region ISecurityDescriptorCmdletProvider method wrappers
        /// Internal wrapper for the GetSecurityDescriptor protected method. This method will
        /// only be called if the provider implements the ISecurityDescriptorCmdletProvider interface.
        /// Nothing. An instance of an object that represents the security descriptor
        /// for the item specified by the path should be written to the context.
            ISecurityDescriptorCmdletProvider permissionProvider = this as ISecurityDescriptorCmdletProvider;
            // if this is not supported, the fn will throw
            CheckIfSecurityDescriptorInterfaceIsSupported(permissionProvider);
            permissionProvider.GetSecurityDescriptor(path, sections);
        /// Internal wrapper for the SetSecurityDescriptor protected method. This method will
        /// The path to the item to set the new security descriptor on.
        /// Nothing. The security descriptor object that was set should be written
            permissionProvider.SetSecurityDescriptor(path, securityDescriptor);
        private static void CheckIfSecurityDescriptorInterfaceIsSupported(ISecurityDescriptorCmdletProvider permissionProvider)
            if (permissionProvider == null)
        #endregion ISecurityDescriptorCmdletProvider method wrappers

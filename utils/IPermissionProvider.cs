    #region ISecurityDescriptorCmdletProvider
    /// Provides an interface that allows simplified interaction
    /// with namespaces that support security descriptors. The methods
    /// on this interface allow a common set of commands to manage the security
    /// on any namespace that supports this interface.
    /// A namespace provider should implement this interface if items in the
    /// namespace are protected by Security Descriptors.
    public interface ISecurityDescriptorCmdletProvider
        /// Gets the security descriptor for the item at the specified path.
        /// The path of the item to from which to retrieve the security descriptor.
        /// The sections of the security descriptor to retrieve, if your provider
        /// supports them.
        /// Nothing.   Write the security descriptor to the context's pipeline for
        /// the item specified by the path using the WriteSecurityDescriptorObject
        void GetSecurityDescriptor(
            AccessControlSections includeSections);
        /// Sets the security descriptor for the item at the specified path.
        /// The path of the item to for which to set the security descriptor.
        /// The new security descriptor for the item.  This should replace the
        /// previously existing security descriptor.
        /// Nothing.   After setting the security descriptor to the value passed in,
        /// write the new security descriptor to the context's pipeline for the
        /// item specified by the path using the WriteSecurityDescriptorObject method.
        void SetSecurityDescriptor(
            ObjectSecurity securityDescriptor);
        /// the item specified by the path.  For example, if "path" points
        /// to a file system directory, the descriptor returned will be
        /// of type DirectorySecurity.
        ObjectSecurity NewSecurityDescriptorFromPath(
        /// This method is used as a convenience function for consumers of
        /// your provider.
        /// The type of Security Descriptor to create.  Your provider should
        /// understand a string representation for each of the types of
        /// SecurityDescriptors that it supports.  For example, the File System
        /// provider performs a case-insensitive comparison against "file" for a
        /// FileSecurity descriptor, and "directory" or "container" for a
        /// DirectorySecurity descriptor.
        ObjectSecurity NewSecurityDescriptorOfType(
    #endregion ISecurityDescriptorCmdletProvider

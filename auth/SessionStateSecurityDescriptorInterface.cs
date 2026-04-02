        /// Gets an instance of an ISecurityDescriptorCmdletProvider given the provider ID.
        /// An instance of a CmdletProvider.
        /// An instance of a ISecurityDescriptorCmdletProvider for the specified provider ID.
        /// ArgumentNullException if providerId is null.
        /// NotSupportedException if the providerId is not for a provider
        /// that is derived from ISecurityDescriptorCmdletProvider.
        internal static ISecurityDescriptorCmdletProvider GetPermissionProviderInstance(CmdletProvider providerInstance)
            if (providerInstance is not ISecurityDescriptorCmdletProvider permissionCmdletProvider)
                        ProviderBaseSecurity.ISecurityDescriptorCmdletProvider_NotSupported);
            return permissionCmdletProvider;
        /// Gets the security descriptor from the specified item.
        /// The path to the item to retrieve the security descriptor from.
        /// <param name="sections">
        /// Specifies the parts of a security descriptor to retrieve.
        /// The security descriptor for the item at the specified path.
        internal Collection<PSObject> GetSecurityDescriptor(string path,
                                                             AccessControlSections sections)
            GetSecurityDescriptor(path, sections, context);
            Collection<PSObject> contextResults = context.GetAccumulatedObjects() ?? new Collection<PSObject>();
            return contextResults;
        /// Nothing. The security descriptor for the item at the specified path is
        internal void GetSecurityDescriptor(
            AccessControlSections sections,
                GetSecurityDescriptor(providerInstance, providerPath, sections, context);
        private void GetSecurityDescriptor(
            // This just verifies that the provider supports the interface.
            GetPermissionProviderInstance(providerInstance);
                providerInstance.GetSecurityDescriptor(path, sections, context);
                    "GetSecurityDescriptorProviderException",
                    SessionStateStrings.GetSecurityDescriptorProviderException,
        /// Sets the security descriptor on the specified item.
        /// The path to the item to set the security descriptor on.
        /// <param name="securityDescriptor">
        /// The security descriptor to set on the item at the specified path.
        /// The security descriptor that was set on the item at the specified path.
        internal Collection<PSObject> SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
            if (securityDescriptor == null)
                throw PSTraceSource.NewArgumentNullException(nameof(securityDescriptor));
            SetSecurityDescriptor(path, securityDescriptor, context);
            // Return an empty array instead of null
        /// Nothing. The security descriptor that was set on the item at the specified path
        /// is written to the context.
        internal void SetSecurityDescriptor(
            ObjectSecurity securityDescriptor,
                SetSecurityDescriptor(
                    securityDescriptor,
        private void SetSecurityDescriptor(
                securityDescriptor != null,
                "Caller should validate securityDescriptor before calling this method");
                providerInstance.SetSecurityDescriptor(path, securityDescriptor, context);
            catch (PrivilegeNotHeldException e)
                // thrown if one tries to set SACL and does not have
                // SeSecurityPrivilege
                context.WriteError(new ErrorRecord(e, e.GetType().FullName, ErrorCategory.PermissionDenied, path));
                // thrown if
                // -- owner or pri. group are invalid OR
                // -- marta returns ERROR_ACCESS_DENIED
                // thrown if path points to an item that does not
                // support access control.
                // for example, FAT or FAT32 file in case of file system provider
                context.WriteError(new ErrorRecord(e, e.GetType().FullName, ErrorCategory.InvalidOperation, path));
            catch (SystemException e)
                // thrown if the CLR gets back unexpected error
                // from OS security or marta
                    "SetSecurityDescriptorProviderException",
                    SessionStateStrings.SetSecurityDescriptorProviderException,
        internal ObjectSecurity NewSecurityDescriptorFromPath(
            ObjectSecurity sd = null;
            // path must resolve to exact 1 item,
            // any other case is an error
                sd = NewSecurityDescriptorFromPath(providerInstance,
                                                   sections);
        private ObjectSecurity NewSecurityDescriptorFromPath(
                ExecutionContext != null,
            ISecurityDescriptorCmdletProvider sdProvider =
                sd = sdProvider.NewSecurityDescriptorFromPath(path,
                    "NewSecurityDescriptorProviderException",
        /// Nothing. The security descriptor for the item at the specified type is
        internal ObjectSecurity NewSecurityDescriptorOfType(
            return NewSecurityDescriptorOfType(providerInstance, type, sections);
                throw PSTraceSource.NewArgumentNullException(nameof(type));
                sd = sdProvider.NewSecurityDescriptorOfType(type,

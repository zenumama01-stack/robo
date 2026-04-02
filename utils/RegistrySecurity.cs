        /// Gets the security descriptor for the item specified by <paramref name="path"/>.
        /// specified by path is written to the WriteSecurityDescriptorObject method.
            IRegistryWrapper key = null;
            // Validate input first.
            key = GetRegkeyForPathWriteIfError(path, false);
                    sd = key.GetAccessControl(sections);
        /// Sets the security descriptor for the item specified by <paramref name="path"/>
            ObjectSecurity sd;
                sd = securityDescriptor as TransactedRegistrySecurity;
                sd = securityDescriptor as RegistrySecurity;
            key = GetRegkeyForPathWriteIfError(path, true);
                // the caller already checks for the following exceptions:
                // -- UnauthorizedAccessException
                // -- PrivilegeNotHeldException
                // -- NotSupportedException
                // -- SystemException
                    key.SetAccessControl(sd);
        /// Creates a new empty security descriptor.
        /// The path to the item whose type is to be used when
        /// creating a new descriptor.
        /// Specifies the parts of a security descriptor to create.
        /// An instance of <see cref="System.Security.AccessControl.ObjectSecurity"/> object.
        /// <remarks><paramref name="path"/> and <paramref name="sections"/> are not used by this method.</remarks>
                return new TransactedRegistrySecurity();
                return new RegistrySecurity(); // sections);
        /// The type of item associated with this security descriptor

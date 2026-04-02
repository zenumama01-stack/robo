    /// Provides the *-SecurityDescriptor noun for the cmdlet providers.
    public sealed class SecurityDescriptorCmdletProviderIntrinsics
        private SecurityDescriptorCmdletProviderIntrinsics()
        /// Initializes a new instance of the SecurityDescriptorCmdletProviderIntrinsics
        /// class, using the Cmdlet parameter to obtain access to the SessionState APIs.
        internal SecurityDescriptorCmdletProviderIntrinsics(Cmdlet cmdlet)
        /// class, using the sessionState parameter to obtain access to the SessionState APIs.
        /// An instance of the real session state class.
        internal SecurityDescriptorCmdletProviderIntrinsics(SessionStateInternal sessionState)
        #region GetSecurityDescriptor
        /// Gets the SecurityDescriptor at the specified path, including only the specified
        /// AccessControlSections.
        /// The path of the item to retrieve. It may be a drive or provider-qualified path and may include.
        /// <param name="includeSections">
        /// The sections of the security descriptor to include.
        /// The SecurityDescriptor(s) at the specified path.
        public Collection<PSObject> Get(string path, AccessControlSections includeSections)
            return _sessionState.GetSecurityDescriptor(path, includeSections);
        /// AccessControlSections, using the provided Context.
        /// The path of the item to retrieve. It may be a drive or provider-qualified path and may include
        internal void Get(string path,
                        AccessControlSections includeSections,
            _sessionState.GetSecurityDescriptor(path, includeSections, context);
        #endregion GetSecurityDescriptor
        #region SetSecurityDescriptor
        /// Sets the provided SecurityDescriptor at the specified path.
        /// The path of the item to set. It may be a drive or provider-qualified path and may include
        /// <param name="sd">
        /// The new security descriptor to set.
        /// The SecurityDescriptor(s) set at the specified path.
        public Collection<PSObject> Set(string path, ObjectSecurity sd)
            Collection<PSObject> result = _sessionState.SetSecurityDescriptor(path, sd);
        /// Sets the SecurityDescriptor at the specified path, using the provided Context.
        internal void Set(string path, ObjectSecurity sd, CmdletProviderContext context)
            _sessionState.SetSecurityDescriptor(path, sd, context);
        #endregion SetSecurityDescriptor
        #region NewSecurityDescriptor
        /// Creates a new SecurityDescriptor from the item at the specified path, including only the specified
        public ObjectSecurity NewFromPath(string path, AccessControlSections includeSections)
            return _sessionState.NewSecurityDescriptorFromPath(path, includeSections);
        /// Creates a new SecurityDescriptor from the specified provider and of the given type,
        /// including only the specified AccessControlSections.
        /// The type of the item which corresponds to the security
        /// descriptor that we want to create.
        /// A new SecurityDescriptor of the specified type.
        public ObjectSecurity NewOfType(string providerId, string type, AccessControlSections includeSections)
            return _sessionState.NewSecurityDescriptorOfType(providerId,
                                                            type,
                                                            includeSections);
        #endregion NewSecurityDescriptor

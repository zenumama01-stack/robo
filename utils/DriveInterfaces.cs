    /// Exposes the Cmdlet Family Provider's drives to the Cmdlet base class. The methods of this class
    /// get and set provider data in session state.
    public sealed class DriveManagementIntrinsics
        private DriveManagementIntrinsics()
        /// Constructs a Drive management facade.
        /// The instance of session state that facade wraps.
        internal DriveManagementIntrinsics(SessionStateInternal sessionState)
        /// Gets the drive information for the current working drive.
        /// This property is readonly. To set the current drive use the
        /// SetLocation method.
        public PSDriveInfo Current
                return _sessionState.CurrentDrive;
        #region New
        /// Creates a new PSDrive in session state.
        /// The drive to be created.
        /// The ID of the scope to create the drive in. This may be one of the scope
        /// keywords like global or local, or it may be an numeric offset of the scope
        /// generation relative to the current scope.
        /// If the scopeID is null or empty the local scope is used.
        /// The drive that was created.
        /// If <paramref name="drive"/> is null.
        /// If the drive already exists,
        /// If <paramref name="drive"/>.Name contains one or more invalid characters; ~ / \\ . :
        /// If the provider is not a DriveCmdletProvider.
        /// The provider for the <paramref name="drive"/> could not be found.
        /// If the provider threw an exception or returned null.
        public PSDriveInfo New(PSDriveInfo drive, string scope)
            return _sessionState.NewDrive(drive, scope);
        /// Creates a new MSH drive in session state.
        /// The context under which this command is running.
        /// Nothing. The drive that is created is written to the context.
        /// If <paramref name="drive"/> or <paramref name="context"/> is null.
        /// If the drive already exists
        internal void New(
            PSDriveInfo drive,
            string scope,
            _sessionState.NewDrive(drive, scope, context);
        /// Gets an object that defines the additional parameters for the NewDrive implementation
        /// for a provider.
        /// <param name="providerId">
        /// The provider ID for the drive that is being created.
        /// The context under which this method is being called.
        /// If the <paramref name="providerId"/> is not a DriveCmdletProvider.
        /// If <paramref name="providerId"/> does not exist.
        internal object NewDriveDynamicParameters(
            string providerId,
            return _sessionState.NewDriveDynamicParameters(providerId, context);
        #endregion New
        #region Remove
        /// Removes the specified drive.
        /// The name of the drive to be removed.
        /// Determines whether drive should be forcefully removed even if there was errors.
        /// The ID of the scope to remove the drive from. This may be one of the scope
        public void Remove(string driveName, bool force, string scope)
            _sessionState.RemoveDrive(driveName, force, scope);
        internal void Remove(
            _sessionState.RemoveDrive(driveName, force, scope, context);
        #endregion Remove
        #region Get
        /// Gets the drive information for the drive specified by name.
        /// The name of the drive to get the drive information for.
        /// The drive information that represents the drive of the specified name.
        /// If <paramref name="driveName"/> is null.
        /// If there is no drive with <paramref name="driveName"/>.
        public PSDriveInfo Get(string driveName)
            return _sessionState.GetDrive(driveName);
        /// The ID of the scope to get the drive from. This may be one of the scope
        /// If <paramref name="scopeID"/> is less than zero or greater than the number of currently
        public PSDriveInfo GetAtScope(string driveName, string scope)
            return _sessionState.GetDrive(driveName, scope);
        /// Retrieves all the drives in the specified scope.
        public Collection<PSDriveInfo> GetAll()
            return _sessionState.Drives(null);
        /// The scope to retrieve the drives from. If null, the
        /// drives in all the scopes will be returned.
        public Collection<PSDriveInfo> GetAllAtScope(string scope)
            return _sessionState.Drives(scope);
        /// Gets all the drives for the specified provider.
        /// The name of the provider to get the drives for.
        /// All the drives in all the scopes for the given provider.
        public Collection<PSDriveInfo> GetAllForProvider(string providerName)
            return _sessionState.GetDrivesForProvider(providerName);
        #endregion GetDrive
        // A private reference to the internal session state of the engine.

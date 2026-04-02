    /// An object that represents a path.
    public sealed class PathInfo
        /// Gets the drive that contains the path.
        public PSDriveInfo Drive
                if (_drive != null &&
                    !_drive.Hidden)
                    result = _drive;
        /// Gets the provider that contains the path.
        /// This is the internal mechanism to get the hidden drive.
        /// The drive associated with this PathInfo.
        internal PSDriveInfo GetDrive()
            return _drive;
        /// Gets the provider internal path for the PSPath that this PathInfo represents.
        /// The provider encountered an error when resolving the path.
        /// The path was a home relative path but the home path was not
        /// set for the provider.
        public string ProviderPath
                if (_providerPath == null)
                    // Construct the providerPath
                    LocationGlobber pathGlobber = _sessionState.Internal.ExecutionContext.LocationGlobber;
                    _providerPath = pathGlobber.GetProviderPath(Path);
                return _providerPath;
        private string _providerPath;
        /// Gets the PowerShell path that this object represents.
                return this.ToString();
        private readonly PSDriveInfo _drive;
        private readonly ProviderInfo _provider;
        /// Gets a string representing the PowerShell path.
        /// A string representing the PowerShell path.
            string result = _path;
            if (_drive == null ||
                _drive.Hidden)
                // For hidden drives just return the current location
                        _path,
                        _provider);
                result = LocationGlobber.GetDriveQualifiedPath(_path, _drive);
        /// The constructor of the PathInfo object.
        /// The drive that contains the path
        /// The provider that contains the path.
        /// The path this object represents.
        /// The session state associated with the drive, provider, and path information.
        /// If <paramref name="drive"/>, <paramref name="provider"/>,
        /// <paramref name="path"/>, or <paramref name="sessionState"/> is null.
        internal PathInfo(PSDriveInfo drive, ProviderInfo provider, string path, SessionState sessionState)
            _drive = drive;

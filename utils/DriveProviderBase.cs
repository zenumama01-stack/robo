    /// The base class for Cmdlet providers that can be exposed through PSDrives.
    /// Although it is possible to derive from this base class to implement a Cmdlet Provider, in most
    /// cases one should derive from <see cref="System.Management.Automation.Provider.ItemCmdletProvider"/>,
    /// <see cref="System.Management.Automation.Provider.ContainerCmdletProvider"/>, or
    /// <see cref ="System.Management.Automation.Provider.NavigationCmdletProvider"/>
    public abstract class DriveCmdletProvider : CmdletProvider
        #region DriveCmdletProvider method wrappers
        /// Internal wrapper for the NewDrive protected method. It is called instead
        /// The PSDriveInfo object the represents the drive to be mounted.
        /// The drive that was returned from the protected NewDrive method.
        internal PSDriveInfo NewDrive(PSDriveInfo drive, CmdletProviderContext context)
            // Make sure the provider supports credentials if they were passed
            // in the drive.
            if (drive.Credential != null &&
                drive.Credential != PSCredential.Empty &&
                !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Credentials, ProviderInfo))
                throw PSTraceSource.NewNotSupportedException(
                    SessionStateStrings.NewDriveCredentials_NotSupported);
            return NewDrive(drive);
        /// the New-PSDrive cmdlet.
        internal object NewDriveDynamicParameters(CmdletProviderContext context)
            return NewDriveDynamicParameters();
        /// Internal wrapper for the RemoveDrive protected method. It is called instead
        /// The PSDriveInfo object the represents the mounted drive.
        /// The drive that was returned from the protected RemoveDrive method.
        internal PSDriveInfo RemoveDrive(PSDriveInfo drive, CmdletProviderContext context)
            return RemoveDrive(drive);
        /// Internal wrapper for the InitializeDefaultDrives protected method. It is called instead
        /// An array of drives returned from the protected InitializeDefaultDrives method.
        internal Collection<PSDriveInfo> InitializeDefaultDrives(CmdletProviderContext context)
            Context.Drive = null;
            return InitializeDefaultDrives();
        #endregion DriveCmdletProvider method wrappers
        #region Protected methods that should be overridden by derived classes
        /// Gives the provider an opportunity to validate the drive
        /// that is being added. It also allows the provider to modify parts
        /// of the PSDriveInfo object. This may be done for performance or
        /// reliability reasons or to provide extra data to all calls using
        /// the Drive.
        /// The proposed new drive.
        /// The new drive that is to be added to the MSH namespace. This
        /// can either be the same <paramref name="drive"/> object that
        /// was passed in or a modified version of it.
        /// The default implementation returns the drive that was passed.
        /// This method gives the provider an opportunity to associate
        /// provider specific data with a drive. This is done by deriving
        /// a new class from <see cref="System.Management.Automation.PSDriveInfo"/>
        /// and adding any properties, methods, or fields that are necessary.
        /// When this method gets called, the override should create an instance
        /// of the derived PSDriveInfo using the passed in PSDriveInfo. The derived
        /// PSDriveInfo should then be returned. Each subsequent call into the provider
        /// that uses this drive will have access to the derived PSDriveInfo via the
        /// PSDriveInfo property provided by the base class.
        /// Any failures should be sent to the <see cref="System.Management.Automation.Provider.CmdletProvider.WriteError(ErrorRecord)"/>
        /// method and null should be returned.
        protected virtual PSDriveInfo NewDrive(PSDriveInfo drive)
        protected virtual object NewDriveDynamicParameters()
        /// Gives the provider an opportunity to clean up any provider specific data
        /// for the drive that is going to be removed.
        /// The Drive object the represents the mounted drive.
        /// If the drive can be removed it should return the drive that was passed
        /// in. If the drive cannot be removed, null should be returned or an exception
        /// should be thrown.
        /// A provider should override this method to free any resources that may be associated with
        /// the drive being removed.
        protected virtual PSDriveInfo RemoveDrive(PSDriveInfo drive)
        /// Gives the provider the ability to map drives after initialization.
        /// A collection of the drives the provider wants to be added to the session upon initialization.
        /// The default implementation returns an empty <see cref="System.Management.Automation.PSDriveInfo"/> collection.
        /// After the Start method is called on a provider, the InitializeDefaultDrives
        /// method is called. This is an opportunity for the provider to
        /// mount drives that are important to it. For instance, the Active Directory
        /// provider might mount a drive for the defaultNamingContext if the
        /// machine is joined to a domain.
        /// All providers should mount a root drive to help the user with discoverability.
        /// This root drive might contain a listing of a set of locations that would be
        /// interesting as roots for other mounted drives. For instance, the Active
        /// Directory provider my create a drive that lists the naming contexts found
        /// in the namingContext attributes on the RootDSE. This will help users
        /// discover interesting mount points for other drives.
        protected virtual Collection<PSDriveInfo> InitializeDefaultDrives()
                return new Collection<PSDriveInfo>();
    #endregion DriveCmdletProvider

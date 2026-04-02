    /// Defines a drive that exposes a provider path to the user.
    /// A cmdlet provider may want to derive from this class to provide their
    /// own public members or to cache information related to the drive. For instance,
    /// if a drive is a connection to a remote machine and making that connection
    /// is expensive, then the provider may want keep a handle to the connection as
    /// a member of their derived <see cref="PSDriveInfo"/> class and use it when
    /// the provider is invoked.
    public class PSDriveInfo : IComparable
        /// using "SessionState" as the category.
        /// This is the same category as the SessionState tracer class.
        [Dbg.TraceSource(
             "PSDriveInfo",
             "The namespace navigation tracer")]
        private static readonly Dbg.PSTraceSource s_tracer =
            Dbg.PSTraceSource.GetTracer("PSDriveInfo",
             "The namespace navigation tracer");
        /// Gets or sets the current working directory for the drive.
        public string CurrentLocation
                return _currentWorkingDirectory;
                _currentWorkingDirectory = value;
        /// The current working directory for the virtual drive
        /// as a relative path from Root.
        private string _currentWorkingDirectory;
        /// Gets the name of the drive.
        /// The name of the virtual drive.
        /// Gets the name of the provider that root path
        /// of the drive represents.
        public ProviderInfo Provider
                return _provider;
        /// The provider information for the provider that implements
        /// the functionality for the drive.
        private ProviderInfo _provider;
        /// Gets the root path of the drive.
                return _root;
                _root = value;
        /// Sets the root of the drive.
        /// The root path to set for the drive.
        /// This method can only be called during drive
        /// creation. A NotSupportedException if this method
        /// is called outside of drive creation.
        /// If this method gets called any other time except
        /// during drive creation.
        internal void SetRoot(string path)
            if (!DriveBeingCreated)
                NotSupportedException e =
                    PSTraceSource.NewNotSupportedException();
            _root = path;
        /// The root of the virtual drive.
        /// Gets or sets the description for the drive.
        /// When supported by provider this specifies a maximum drive size.
        public long? MaximumSize { get; internal set; }
        /// Gets the credential to use with the drive.
        public PSCredential Credential { get; } = PSCredential.Empty;
        /// Determines if the root of the drive can
        /// be modified during drive creation through
        /// the SetRoot method.
        /// True if the drive is being created and the
        /// root can be modified through the SetRoot method.
        internal bool DriveBeingCreated { get; set; }
        /// True if the drive was automounted by the system,
        internal bool IsAutoMounted { get; set; }
        /// and then manually removed by the user.
        internal bool IsAutoMountedManuallyRemoved { get; set; }
        internal bool Persist { get; } = false;
        /// Get or sets the value indicating if the created drive is a network drive.
        internal bool IsNetworkDrive { get; set; } = false;
        /// Gets or sets the UNC path of the drive. This property would be populated only
        /// if the created PSDrive is targeting a network drive or else this property
        /// would be null.
        public string DisplayRoot { get; internal set; } = null;
        /// Gets or sets if the drive-root relative paths on this drive are separated by a
        /// colon or not.
        /// This is true for all PSDrives on all platforms, except for filesystems on
        /// non-Windows platforms.
        /// This is not a path separator in the sense of separating paths in a single
        /// The biggest difference in filesystem handling between PS internally, and Unix
        /// style systems is, that paths on Windows separate the drive letter from the
        /// actual path by a colon. The second difference is, that a path that starts with
        /// a \ or / on Windows is considered to be a relative path (drive-relative in
        /// that case) where a similar path on a Unix style filesystem would be
        /// root-relative, which is basically drive-relative for the filesystem, as there
        /// is only one filesystem drive.
        /// This property indicates, that a path can be checked for that drive-relativity
        /// by checking for a colon. The main reason for this can be seen in all the
        /// places that use this property, where PowerShell's code checks/splits/string
        /// manipulates paths according to the colon character. This happens in many
        /// places.
        /// The idea here was to introduce a property that allows a code to query if a
        /// PSDrive expects colon to be such a separator or not. I talked to Jim back then
        /// about the problem, and this seemed to be a reasonable solution, given that
        /// there is no other way to know for a PSDrive if paths can be qualified only in
        /// a certain windows way on all platforms, or need special treatment on platforms
        /// where colon does not exist as drive separator (regular filesystems on Unix
        /// platforms are the only exception).
        /// Globally this property can also be only true for one single PSDrive, because
        /// if there is no drive separator, there is also no drive, and because there is
        /// no drive there is no way to match against multiple such drives.
        /// Additional data:
        /// It seems that on single rooted filesystems, only the default
        /// drive of "/" needs to set this VolumeSeparatedByColon to false
        /// otherwise, creating new drives from the filesystem should actually
        /// have this set to true as all the drives will have <string>: except
        /// for "/"
        public bool VolumeSeparatedByColon { get; internal set; } = true;
        /// Constructs a new instance of the PSDriveInfo using another PSDriveInfo
        /// as a template.
        /// <param name="driveInfo">
        /// An existing PSDriveInfo object that should be copied to this instance.
        /// A protected constructor that derived classes can call with an instance
        /// of this class. This allows for easy creation of derived PSDriveInfo objects
        /// which can be created in CmdletProvider's NewDrive method using the PSDriveInfo
        /// that is passed in.
        /// If <paramref name="PSDriveInfo"/> is null.
        protected PSDriveInfo(PSDriveInfo driveInfo)
            if (driveInfo == null)
                throw PSTraceSource.NewArgumentNullException(nameof(driveInfo));
            _name = driveInfo.Name;
            _provider = driveInfo.Provider;
            Credential = driveInfo.Credential;
            _currentWorkingDirectory = driveInfo.CurrentLocation;
            Description = driveInfo.Description;
            this.MaximumSize = driveInfo.MaximumSize;
            DriveBeingCreated = driveInfo.DriveBeingCreated;
            _hidden = driveInfo._hidden;
            IsAutoMounted = driveInfo.IsAutoMounted;
            _root = driveInfo._root;
            Persist = driveInfo.Persist;
            this.Trace();
        /// Constructs a drive that maps a PowerShell Path in
        /// the shell to a Cmdlet Provider.
        /// The name of the provider which implements the functionality
        /// for the root path of the drive.
        /// The root path of the drive. For example, the root of a
        /// drive in the file system can be c:\windows\system32
        /// <param name="description">
        /// The description for the drive.
        /// <param name="credential">
        /// The credentials under which all operations on the drive should occur.
        /// If null, the current user credential is used.
        /// <throws>
        /// ArgumentNullException - if <paramref name="name"/>,
        /// <paramref name="provider"/>, or <paramref name="root"/>
        /// is null.
        /// </throws>
        public PSDriveInfo(
            ProviderInfo provider,
            string root,
            string description,
            PSCredential credential)
            if (provider == null)
                throw PSTraceSource.NewArgumentNullException(nameof(provider));
            if (root == null)
                throw PSTraceSource.NewArgumentNullException(nameof(root));
            // Copy the parameters to the local members
            _root = root;
                Credential = credential;
            // Set the current working directory to the empty
            // string since it is relative to the root.
            _currentWorkingDirectory = string.Empty;
                _currentWorkingDirectory != null,
                "The currentWorkingDirectory cannot be null");
            // Trace out the fields
        /// <param name="displayRoot">
        /// The network path of the drive. This field would be populated only if PSDriveInfo
        /// is targeting the network drive or else this filed is null for local drives.
            PSCredential credential, string displayRoot)
            : this(name, provider, root, description, credential)
            DisplayRoot = displayRoot;
        /// <param name="persist">
        /// It indicates if the created PSDrive would be
        /// persisted across PowerShell sessions.
            bool persist)
            Persist = persist;
        /// Gets the name of the drive as a string.
        /// Returns a String that is that name of the drive.
        /// Gets or sets the hidden property. The hidden property
        /// determines if the drive should be hidden from the user.
        /// True if the drive should be hidden from the user, false
        internal bool Hidden
                return _hidden;
                _hidden = value;
        /// Determines if the drive should be hidden from the user.
        private bool _hidden;
        /// Sets the name of the drive to a new name.
        /// The new name for the drive.
        /// This must be internal so that we allow the renaming of drives
        /// via the Core Command API but not through a reference to the
        /// drive object. More goes in to renaming a drive than just modifying
        /// the name in this class.
        internal void SetName(string newName)
            if (string.IsNullOrEmpty(newName))
                throw PSTraceSource.NewArgumentException(nameof(newName));
            _name = newName;
        /// Sets the provider of the drive to a new provider.
        /// <param name="newProvider">
        /// The new provider for the drive.
        /// This must be internal so that we allow the renaming of providers.
        /// All drives must be associated with the new provider name and can
        /// be changed using the Core Command API but not through a reference to the
        /// drive object. More goes in to renaming a provider than just modifying
        /// the provider in this class.
        /// If <paramref name="newProvider"/> is null.
        internal void SetProvider(ProviderInfo newProvider)
            if (newProvider == null)
                throw PSTraceSource.NewArgumentNullException(nameof(newProvider));
            _provider = newProvider;
        /// Traces the virtual drive.
        internal void Trace()
                "A drive was found:");
                    "\tName: {0}",
                    Name);
            if (Provider != null)
                    "\tProvider: {0}",
                    Provider);
            if (Root != null)
                    "\tRoot: {0}",
            if (CurrentLocation != null)
                    "\tCWD: {0}",
                    CurrentLocation);
                    "\tDescription: {0}",
                    Description);
        /// Compares this instance to the specified drive.
        /// <param name="drive">
        /// A PSDriveInfo object to compare.
        /// A signed number indicating the relative values of this instance and object specified.
        /// Return Value: Less than zero        Meaning: This instance is less than object.
        /// Return Value: Zero                  Meaning: This instance is equal to object.
        /// Return Value: Greater than zero     Meaning: This instance is greater than object or object is a null reference.
        public int CompareTo(PSDriveInfo drive)
                throw PSTraceSource.NewArgumentNullException(nameof(drive));
            return string.Compare(Name, drive.Name, StringComparison.OrdinalIgnoreCase);
        /// Compares this instance to the specified object. The object must be a PSDriveInfo.
        /// <param name="obj">
        /// An object to compare.
        /// A signed number indicating the relative values of this
        /// instance and object specified.
        /// If <paramref name="obj"/> is not a PSDriveInfo instance.
        public int CompareTo(object obj)
            PSDriveInfo drive = obj as PSDriveInfo;
                        nameof(obj),
                        SessionStateStrings.OnlyAbleToComparePSDriveInfo);
            return (CompareTo(drive));
        /// Compares this instance to the specified object.
        /// True if the drive names are equal, false otherwise.
            if (obj is PSDriveInfo)
                return CompareTo(obj) == 0;
        public bool Equals(PSDriveInfo drive)
            return CompareTo(drive) == 0;
        /// Equality operator for the drive determines if the drives
        /// are equal by having the same name.
        /// <param name="drive1">
        /// The first object to compare to the second.
        /// <param name="drive2">
        /// The second object to compare to the first.
        /// True if the objects are PSDriveInfo objects and have the same name,
        public static bool operator ==(PSDriveInfo drive1, PSDriveInfo drive2)
            object drive1Object = drive1;
            object drive2Object = drive2;
            if ((drive1Object == null) == (drive2Object == null))
                if (drive1Object != null)
                    return drive1.Equals(drive2);
        /// Inequality operator for the drive determines if the drives
        /// are not equal by using the drive name.
        /// True if the PSDriveInfo objects do not have the same name,
        public static bool operator !=(PSDriveInfo drive1, PSDriveInfo drive2)
            return !(drive1 == drive2);
        /// Compares the specified drives to determine if drive1 is less than
        /// drive2.
        /// The drive to determine if it is less than the other drive.
        /// The drive to compare drive1 against.
        /// True if the lexical comparison of drive1's name is less than drive2's name.
        public static bool operator <(PSDriveInfo drive1, PSDriveInfo drive2)
            if (drive1Object == null)
                return (drive2Object != null);
                if (drive2Object == null)
                    // Since drive1 is not null and drive2 is, drive1 is greater than drive2
                    // Since drive1 and drive2 are not null use the CompareTo
                    return drive1.CompareTo(drive2) < 0;
        /// Compares the specified drives to determine if drive1 is greater than
        /// The drive to determine if it is greater than the other drive.
        /// True if the lexical comparison of drive1's name is greater than drive2's name.
        public static bool operator >(PSDriveInfo drive1, PSDriveInfo drive2)
            if ((drive1Object == null))
                // Since both drives are null, they are equal
                // Since drive1 is null it is less than drive2 which is not null
                    return drive1.CompareTo(drive2) > 0;
        /// Gets the hash code for this instance.
        /// <returns>The result of base.GetHashCode().</returns>
        /// <!-- Override the base GetHashCode because the compiler complains
        /// if you don't when you implement operator== and operator!= -->
            return base.GetHashCode();
        private PSNoteProperty _noteProperty;
        internal PSNoteProperty GetNotePropertyForProviderCmdlets(string name)
            if (_noteProperty == null)
                Interlocked.CompareExchange(ref _noteProperty,
                                            new PSNoteProperty(name, this), null);
            return _noteProperty;

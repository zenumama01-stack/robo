    /// Describes a Local User.
    /// Objects of this type are provided to and returned from user-related Cmdlets.
    public class LocalUser : LocalPrincipal
        /// The date and time at which this user account expires.
        /// A value of null indicates that the account never expires.
        public DateTime? AccountExpires { get; set; }
        /// A short description of the User.
        /// Indicates whether the user account is enabled (true) or disabled (false).
        public bool Enabled { get; set; }
        /// The user's full name. Not the same as the User name.
        public string FullName { get; set; }
        /// The date and time at which this user account password is allowed
        /// to be changed. The password cannot be changed before this time.
        /// A value of null indicates that the password can be changed anytime.
        public DateTime? PasswordChangeableDate { get; set; }
        /// The date and time at which this user account password must be changed
        /// to a new password. A value of null indicates that the password will
        /// never expire.
        public DateTime? PasswordExpires { get; set; }
        /// Indicates whether the user is allowed to change the password (true)
        /// or not (false).
        public bool UserMayChangePassword { get; set; }
        /// Indicates whether the user must have a password (true) or not (false).
        public bool PasswordRequired { get; set; }
        /// The date and time at which this user last changed the account password.
        public DateTime? PasswordLastSet { get; set; }
        /// The date and time at which the user last logged on to the machine.
        public DateTime? LastLogon { get; set; }
        /// Initializes a new LocalUser object.
        public LocalUser()
            ObjectClass = Strings.ObjectClassUser;
        /// <param name="name">Name of the new LocalUser.</param>
        public LocalUser(string name)
        /// Construct a new LocalUser object that is a copy of another.
        /// <param name="other">The LocalUser object to copy.</param>
        private LocalUser(LocalUser other)
            SID = other.SID;
            PrincipalSource = other.PrincipalSource;
            ObjectClass = other.ObjectClass;
            AccountExpires = other.AccountExpires;
            Enabled = other.Enabled;
            FullName = other.FullName;
            PasswordChangeableDate = other.PasswordChangeableDate;
            PasswordExpires = other.PasswordExpires;
            UserMayChangePassword = other.UserMayChangePassword;
            PasswordRequired = other.PasswordRequired;
            PasswordLastSet = other.PasswordLastSet;
            LastLogon = other.LastLogon;
        /// Provides a string representation of the LocalUser object.
        /// A string containing the User Name.
        /// Create a copy of a LocalUser object.
        /// A new LocalUser object with the same property values as this one.
        public LocalUser Clone()
            return new LocalUser(this);

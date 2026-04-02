    /// The Set-LocalUser cmdlet changes the properties of a user account in the
    /// local Windows Security Accounts Manager. It can also reset the password of a
    /// local user account.
    [Cmdlet(VerbsCommon.Set, "LocalUser",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717984")]
    [Alias("slu")]
    public class SetLocalUserCommand : PSCmdlet
                "UserMayChangePassword",
                "PasswordNeverExpires"
        /// Specifies when the user account will expire. Set to null to indicate that
        /// the account will never expire. The default value is null (account never
        /// expires).
        /// Specifies the of the local user account to modify in the local Security
        /// Specifies the local user account to change.
                   ParameterSetName = "Name")]
        /// Specifies the password for the local user account.
        public bool PasswordNeverExpires
        private bool passwordneverexpires;
        /// The following is the definition of the input parameter "UserMayChangePassword".
        public bool UserMayChangePassword
            get { return this.usermaychangepassword;}
            set { this.usermaychangepassword = value; }
        private bool usermaychangepassword;
                LocalUser user = null;
                        user = InputObject;
                    user = sam.GetLocalUser(Name);
                        user = null;
                    user = sam.GetLocalUser(SID);
                if (user == null)
                // We start with what already exists
                var delta = user.Clone();
                bool? passwordNeverExpires = null;
                                delta.AccountExpires = this.AccountExpires;
                            case "Description":
                                delta.Description = this.Description;
                            case "FullName":
                                delta.FullName = this.FullName;
                            case "UserMayChangePassword":
                                delta.UserMayChangePassword = this.UserMayChangePassword;
                            case "PasswordNeverExpires":
                                passwordNeverExpires = this.PasswordNeverExpires;
                    delta.AccountExpires = null;
                sam.UpdateLocalUser(user, delta, Password, passwordNeverExpires);
            return ShouldProcess(target, Strings.ActionSetUser);

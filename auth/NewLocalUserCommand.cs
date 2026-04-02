    /// The New-LocalUser cmdlet creates a new local user account.
    [Cmdlet(VerbsCommon.New, "LocalUser",
            DefaultParameterSetName = "Password",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717981")]
    [Alias("nlu")]
    public class NewLocalUserCommand : PSCmdlet
        // Names of object- and boolean-type parameters.
        // Switch parameters don't need to be included.
        private static string[] parameterNames = new string[]
                "AccountExpires",
                "Description",
                "Disabled",
                "FullName",
                "UserMayNotChangePassword"
        /// The following is the definition of the input parameter "AccountExpires".
        /// Specifies when the user account will expire.
        public System.DateTime AccountExpires
            get { return this.accountexpires;}
            set { this.accountexpires = value; }
        private System.DateTime accountexpires;
        // This parameter added by hand (copied from SetLocalUserCommand), not by Cmdlet Designer
        /// The following is the definition of the input parameter "AccountNeverExpires".
        /// Specifies that the account will not expire.
        public System.Management.Automation.SwitchParameter AccountNeverExpires
            get { return this.accountneverexpires;}
            set { this.accountneverexpires = value; }
        private System.Management.Automation.SwitchParameter accountneverexpires;
        /// A descriptive comment for this user account.
        /// The following is the definition of the input parameter "Disabled".
        /// Specifies whether this user account is enabled or disabled.
        public System.Management.Automation.SwitchParameter Disabled
            get { return this.disabled;}
            set { this.disabled = value; }
        private System.Management.Automation.SwitchParameter disabled;
        /// The following is the definition of the input parameter "FullName".
        /// Specifies the full name of the user account. This is different from the
        /// username of the user account.
        public string FullName
            get { return this.fullname;}
            set { this.fullname = value; }
        private string fullname;
        /// Specifies the user name for the local user account. This can be a local user
        /// account or a local user account that is connected to a Microsoft Account.
        [ValidateLength(1, 20)]
        /// The following is the definition of the input parameter "Password".
        /// Specifies the password for the local user account. A password can contain up
        /// to 127 characters.
                   ParameterSetName = "Password",
        public System.Security.SecureString Password
            get { return this.password;}
            set { this.password = value; }
        private System.Security.SecureString password;
        /// The following is the definition of the input parameter "PasswordChangeableDate".
        /// Specifies that the new User account has no password.
                   ParameterSetName = "NoPassword",
        public System.Management.Automation.SwitchParameter NoPassword
            get { return this.nopassword; }
            set { this.nopassword = value; }
        private System.Management.Automation.SwitchParameter nopassword;
        /// The following is the definition of the input parameter "PasswordNeverExpires".
        /// Specifies that the password will not expire.
        [Parameter(ParameterSetName = "Password",
        public System.Management.Automation.SwitchParameter PasswordNeverExpires
            get { return this.passwordneverexpires; }
            set { this.passwordneverexpires = value; }
        private System.Management.Automation.SwitchParameter passwordneverexpires;
        /// The following is the definition of the input parameter "UserMayNotChangePassword".
        /// Specifies whether the user is allowed to change the password on this
        /// account. The default value is True.
        public System.Management.Automation.SwitchParameter UserMayNotChangePassword
            get { return this.usermaynotchangepassword;}
            set { this.usermaynotchangepassword = value; }
        private System.Management.Automation.SwitchParameter usermaynotchangepassword;
            if (this.HasParameter("AccountExpires") && AccountNeverExpires.IsPresent)
                InvalidParametersException ex = new InvalidParametersException("AccountExpires", "AccountNeverExpires");
                ThrowTerminatingError(ex.MakeErrorRecord());
                    var user = new LocalUser
                                    Name = Name,
                                    Enabled = true,
                                    FullName = FullName,
                                    UserMayChangePassword = true
                    foreach (var paramName in parameterNames)
                        if (this.HasParameter(paramName))
                            switch (paramName)
                                case "AccountExpires":
                                    user.AccountExpires = AccountExpires;
                                case "Disabled":
                                    user.Enabled = !Disabled;
                                case "UserMayNotChangePassword":
                                    user.UserMayChangePassword = !UserMayNotChangePassword;
                    if (AccountNeverExpires.IsPresent)
                        user.AccountExpires = null;
                    // Password will be null if NoPassword was given
                    user = sam.CreateLocalUser(user, Password, PasswordNeverExpires.IsPresent);
            return ShouldProcess(target, Strings.ActionNewUser);

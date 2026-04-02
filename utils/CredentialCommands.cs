    /// Defines the implementation of the 'get-credential' cmdlet.
    /// The get-credential Cmdlet establishes a credential object called a
    /// PSCredential, by pairing a given username with
    /// a prompted password. That credential object can then be used for other
    /// operations involving security.
    [Cmdlet(VerbsCommon.Get, "Credential", DefaultParameterSetName = GetCredentialCommand.credentialSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096824")]
    [OutputType(typeof(PSCredential), ParameterSetName = new string[] { GetCredentialCommand.credentialSet, GetCredentialCommand.messageSet })]
    public sealed class GetCredentialCommand : PSCmdlet
        /// The Credential parameter set name.
        private const string credentialSet = "CredentialSet";
        /// The Message parameter set name.
        private const string messageSet = "MessageSet";
        /// Gets or sets the underlying PSCredential of
        /// the instance.
        [Parameter(Position = 0, ParameterSetName = credentialSet)]
        /// Gets and sets the user supplied message providing description about which script/function is
        /// requesting the PSCredential from the user.
        [Parameter(Mandatory = false, ParameterSetName = messageSet)]
            get { return _message; }
            set { _message = value; }
        private string _message = UtilsStrings.PromptForCredential_DefaultMessage;
        /// Gets and sets the user supplied username to be used while creating the PSCredential.
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = messageSet)]
        public string UserName
            get { return _userName; }
            set { _userName = value; }
        private string _userName = null;
        /// Gets and sets the title on the window prompt.
            get { return _title; }
            set { _title = value; }
        private string _title = UtilsStrings.PromptForCredential_DefaultCaption;
        /// Initializes a new instance of the GetCredentialCommand
        public GetCredentialCommand() : base()
        /// The command outputs the stored PSCredential.
                WriteObject(Credential);
                Credential = this.Host.UI.PromptForCredential(_title, _message, _userName, string.Empty);
                    "CouldNotPromptForCredential",

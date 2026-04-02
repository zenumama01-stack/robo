    /// This Cmdlet enables the IT Pro to create a CIM Session. CIM Session object
    /// is a client-side representation of the connection between the client and the
    /// server.
    /// The CimSession object returned by the Cmdlet is used by all other CIM
    /// cmdlets.
    [Alias("ncms")]
    [Cmdlet(VerbsCommon.New, "CimSession", DefaultParameterSetName = CredentialParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227967")]
    public sealed class NewCimSessionCommand : CimBaseCommand
        #region cmdlet parameters
        /// The following is the definition of the input parameter "Authentication".
        /// The following is the validation set for allowed authentication types.
            ParameterSetName = CredentialParameterSet)]
        public PasswordAuthenticationMechanism Authentication
                return authentication;
                authentication = value;
                authenticationSet = true;
        private PasswordAuthenticationMechanism authentication;
        private bool authenticationSet = false;
        /// The following is the definition of the input parameter "Credential".
        /// Specifies a user account that has permission to perform this action.
        /// The default is the current user.
        [Parameter(Position = 1, ParameterSetName = CredentialParameterSet)]
        [Credential]
        public PSCredential Credential { get; set; }
        /// The following is the definition of the input parameter "CertificateThumbprint".
        /// This is specificly for wsman protocol.
                   ParameterSetName = CertificateParameterSet)]
        public string CertificateThumbprint { get; set; }
        /// Specifies the computer on which the commands associated with this session
        /// will run. The default value is LocalHost.
        [ValidateNotNullOrEmpty]
        public string[] ComputerName { get; set; }
        /// Specifies a friendly name for the CIM Session connection.
        /// where <int> is the next available session number. Example, CimSession1,
        /// CimSession2, etc...
        public string Name { get; set; }
        /// Specifies the operation timeout for all operations in session. Individual
        /// operations can override this timeout.
        /// The unit is Second.
        public uint OperationTimeoutSec
                return operationTimeout;
                operationTimeout = value;
                operationTimeoutSet = true;
        internal bool operationTimeoutSet = false;
        /// The following is the definition of the input parameter "SkipTestConnection".
        /// Specifies where test connection should be skipped
        public SwitchParameter SkipTestConnection { get; set; }
        /// The following is the definition of the input parameter "Port".
        /// Specifies the port number to use, if different than the default port number.
        public uint Port
                return port;
                port = value;
                portSet = true;
        private uint port;
        private bool portSet = false;
        /// The following is the definition of the input parameter "SessionOption".
        /// Specifies the SessionOption object that is passed to the Cmdlet as argument.
        /// If the argument is not given, a default SessionOption will be created for
        /// the session in .NET API layer.
        /// If a <see cref="DCOMSessionOption"/> object is passed, then
        /// connection is made using DCOM. If a <see cref="WsManSessionOption"/>
        /// object is passed, then connection is made using WsMan.
        public Microsoft.Management.Infrastructure.Options.CimSessionOptions SessionOption { get; set; }
            cimNewSession = new CimNewSession();
            this.CmdletOperation = new CmdletOperationTestCimSession(this, this.cimNewSession);
            CimSessionOptions outputOptions;
            CimCredential outputCredential;
            BuildSessionOptions(out outputOptions, out outputCredential);
            cimNewSession.NewCimSession(this, outputOptions, outputCredential);
            cimNewSession.ProcessActions(this.CmdletOperation);
            cimNewSession.ProcessRemainActions(this.CmdletOperation);
        /// Build a CimSessionOptions, used to create CimSession.
        /// <returns>Null means no prefer CimSessionOptions.</returns>
        internal void BuildSessionOptions(out CimSessionOptions outputOptions, out CimCredential outputCredential)
            CimSessionOptions options = null;
            if (this.SessionOption != null)
                // clone the sessionOption object
                if (this.SessionOption is WSManSessionOptions)
                    options = new WSManSessionOptions(this.SessionOption as WSManSessionOptions);
                    options = new DComSessionOptions(this.SessionOption as DComSessionOptions);
            outputOptions = null;
            outputCredential = null;
            if (options != null)
                if (options is DComSessionOptions dcomOptions)
                    bool conflict = false;
                    string parameterName = string.Empty;
                    if (this.CertificateThumbprint != null)
                        conflict = true;
                        parameterName = @"CertificateThumbprint";
                    if (portSet)
                        parameterName = @"Port";
                    if (conflict)
                        ThrowConflictParameterWasSet(@"New-CimSession", parameterName, @"DComSessionOptions");
            if (portSet || (this.CertificateThumbprint != null))
                WSManSessionOptions wsmanOptions = (options == null) ? new WSManSessionOptions() : options as WSManSessionOptions;
                    wsmanOptions.DestinationPort = this.Port;
                    portSet = false;
                    CimCredential credentials = new(CertificateAuthenticationMechanism.Default, this.CertificateThumbprint);
                    wsmanOptions.AddDestinationCredentials(credentials);
                options = wsmanOptions;
            if (this.operationTimeoutSet)
                    options.Timeout = TimeSpan.FromSeconds((double)this.OperationTimeoutSec);
            if (this.authenticationSet || (this.Credential != null))
                PasswordAuthenticationMechanism authentication = this.authenticationSet ? this.Authentication : PasswordAuthenticationMechanism.Default;
                if (this.authenticationSet)
                    this.authenticationSet = false;
                CimCredential credentials = CreateCimCredentials(this.Credential, authentication, @"New-CimSession", @"Authentication");
                if (credentials == null)
                DebugHelper.WriteLog("Credentials: {0}", 1, credentials);
                outputCredential = credentials;
                    DebugHelper.WriteLog("Add credentials to option: {0}", 1, options);
                    options.AddDestinationCredentials(credentials);
            DebugHelper.WriteLogEx("Set outputOptions: {0}", 1, outputOptions);
            outputOptions = options;
        /// CimNewSession object
        private CimNewSession cimNewSession;
        protected override void DisposeInternal()
            base.DisposeInternal();
            this.cimNewSession?.Dispose();

    #region Test-WSMAN
    /// Issues an operation against the remote machine to ensure that the wsman
    /// service is running.
    [Cmdlet(VerbsDiagnostic.Test, "WSMan", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2097114")]
    public class TestWSManCommand : AuthenticatingWSManCommand, IDisposable
        /// Executes the management operation on the specified computer. The default is
        /// the local computer. Type the fully qualified domain name, NETBIOS name or IP
        /// address to indicate the remote host.
        /// from. The available method are an enum called AuthenticationMechanism in the
        /// Overriding to use a different default than the one in AuthenticatingWSManCommand base class
        [Alias("auth", "am")]
        public override AuthenticationMechanism Authentication
                ValidateSpecifiedAuthentication();
        private AuthenticationMechanism authentication = AuthenticationMechanism.None;
            string connectionStr = string.Empty;
            connectionStr = helper.CreateConnectionString(null, port, computername, applicationname);
                m_SessionObj = helper.CreateSessionObject(wsmanObject, Authentication, null, Credential, connectionStr, CertificateThumbprint, usessl.IsPresent);
                m_SessionObj.Timeout = 1000; // 1 sec. we are putting this low so that Test-WSMan can return promptly if the server goes unresponsive.
                xmldoc.LoadXml(m_SessionObj.Identify(0));
                        ErrorDoc.LoadXml(m_SessionObj.Error);
                        InvalidOperationException ex = new InvalidOperationException(ErrorDoc.OuterXml);
                        ErrorRecord er = new ErrorRecord(ex, "WsManError", ErrorCategory.InvalidOperation, computername);
                        this.WriteError(er);

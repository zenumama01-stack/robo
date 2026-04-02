    #region Base class for cmdlets taking credential, authentication, certificatethumbprint
    /// Common base class for all WSMan cmdlets that
    /// take Authentication, CertificateThumbprint and Credential parameters.
    public class AuthenticatingWSManCommand : PSCmdlet
        /// Specifies a user account that has permission to perform this action. The
        /// default is the current user.
        [Alias("cred", "c")]
        public virtual PSCredential Credential
                return credential;
                credential = value;
        private PSCredential credential;
        public virtual AuthenticationMechanism Authentication
        public virtual string CertificateThumbprint
                return thumbPrint;
                thumbPrint = value;
        internal void ValidateSpecifiedAuthentication()
            WSManHelper.ValidateSpecifiedAuthentication(
                this.Authentication,
                this.Credential,
                this.CertificateThumbprint);
    #region Connect-WsMan
    /// Connect wsman cmdlet.
    [Cmdlet(VerbsCommunications.Connect, "WSMan", DefaultParameterSetName = "ComputerName", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096841")]
    public class ConnectWSManCommand : AuthenticatingWSManCommand
        [Parameter(ParameterSetName = "ComputerName", Position = 0)]
            if (connectionuri != null)
                    // always in the format http://server:port/applicationname
                    string[] constrsplit = connectionuri.OriginalString.Split(":" + port + "/" + applicationname, StringSplitOptions.None);
                    computername = constrsplit1[1].Trim();
                catch (IndexOutOfRangeException)
                    helper.AssertError(helper.GetResourceMsgFromResourcetext("NotProperURI"), false, connectionuri);
            string crtComputerName = computername ?? "localhost";
            if (this.SessionState.Path.CurrentProviderLocation(WSManStringLiterals.rootpath).Path.StartsWith(this.SessionState.Drive.Current.Name + ":" + WSManStringLiterals.DefaultPathSeparator + crtComputerName, StringComparison.OrdinalIgnoreCase))
                helper.AssertError(helper.GetResourceMsgFromResourcetext("ConnectFailure"), false, computername);
            helper.CreateWsManConnection(ParameterSetName, connectionuri, port, computername, applicationname, usessl.IsPresent, Authentication, sessionoption, Credential, CertificateThumbprint);
    #region Disconnect-WSMAN
    [Cmdlet(VerbsCommunications.Disconnect, "WSMan", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096839")]
    public class DisconnectWSManCommand : PSCmdlet, IDisposable
        Dispose(object session)
            session = null;
            computername ??= "localhost";
            if (this.SessionState.Path.CurrentProviderLocation(WSManStringLiterals.rootpath).Path.StartsWith(WSManStringLiterals.rootpath + ":" + WSManStringLiterals.DefaultPathSeparator + computername, StringComparison.OrdinalIgnoreCase))
                helper.AssertError(helper.GetResourceMsgFromResourcetext("DisconnectFailure"), false, computername);
            if (computername.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                helper.AssertError(helper.GetResourceMsgFromResourcetext("LocalHost"), false, computername);
            object _ws = helper.RemoveFromDictionary(computername);
            if (_ws != null)
                Dispose(_ws);
                helper.AssertError(helper.GetResourceMsgFromResourcetext("InvalidComputerName"), false, computername);
    #endregion Disconnect-WSMAN

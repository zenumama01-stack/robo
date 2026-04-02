    #region SendMailMessage
    /// Implementation for the Send-MailMessage command.
    [Obsolete("This cmdlet does not guarantee secure connections to SMTP servers. While there is no immediate replacement available in PowerShell, we recommend you do not use Send-MailMessage at this time. See https://aka.ms/SendMailMessage for more information.")]
    [Cmdlet(VerbsCommunications.Send, "MailMessage", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097115")]
    public sealed class SendMailMessage : PSCmdlet
        /// Gets or sets the files names to be attached to the email.
        /// If the filename specified can not be found, then the relevant error
        /// message should be thrown.
        [Alias("PsPath")]
        public string[] Attachments { get; set; }
        /// Gets or sets the address collection that contains the
        /// blind carbon copy (BCC) recipients for the e-mail message.
        public string[] Bcc { get; set; }
        /// Gets or sets the body (content) of the message.
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Body { get; set; }
        /// Gets or sets the value indicating whether the mail message body is in Html.
        [Alias("BAH")]
        public SwitchParameter BodyAsHtml { get; set; }
        /// Gets or sets the encoding used for the content of the body and also the subject.
        /// This is set to ASCII to ensure there are no problems with any email server.
        [Alias("BE")]
        private Encoding _encoding = Encoding.ASCII;
        /// carbon copy (CC) recipients for the e-mail message.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Cc")]
        public string[] Cc { get; set; }
        /// Gets or sets the delivery notifications options for the e-mail message. The various
        /// options available for this parameter are None, OnSuccess, OnFailure, Delay and Never.
        [Alias("DNO")]
        public DeliveryNotificationOptions DeliveryNotificationOption { get; set; }
        /// Gets or sets the from address for this e-mail message. The default value for
        /// this parameter is the email address of the currently logged on user.
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string From { get; set; }
        /// Gets or sets the name of the Host used to send the email. This host name will be assigned
        /// to the Powershell variable PSEmailServer, if this host can not reached an appropriate error.
        /// message will be displayed.
        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true)]
        public string SmtpServer { get; set; }
        /// Gets or sets the priority of the email message. The valid values for this are Normal, High and Low.
        public MailPriority Priority { get; set; }
        /// Gets or sets the Reply-To field for this e-mail message.
        public string[] ReplyTo { get; set; }
        /// Gets or sets the subject of the email message.
        [Parameter(Mandatory = false, Position = 1, ValueFromPipelineByPropertyName = true)]
        [Alias("sub")]
        public string Subject { get; set; }
        /// Gets or sets the To address for this e-mail message.
        public string[] To { get; set; }
        /// Gets or sets the credential for this e-mail message.
        /// Gets or sets if Secured layer is required or not.
        public SwitchParameter UseSsl { get; set; }
        /// Gets or sets the Port to be used on the server. <see cref="SmtpServer"/>
        /// Value must be greater than zero.
        public int Port { get; set; }
        #region Private variables and methods
        // Instantiate a new instance of MailMessage
        private readonly MailMessage _mMailMessage = new();
        private SmtpClient _mSmtpClient = null;
        /// Add the input addresses which are either string or hashtable to the MailMessage.
        /// It returns true if the from parameter has more than one value.
        /// <param name="address"></param>
        /// <param name="param"></param>
        private void AddAddressesToMailMessage(object address, string param)
            string[] objEmailAddresses = address as string[];
            foreach (string strEmailAddress in objEmailAddresses)
                    switch (param)
                        case "to":
                                _mMailMessage.To.Add(new MailAddress(strEmailAddress));
                        case "cc":
                                _mMailMessage.CC.Add(new MailAddress(strEmailAddress));
                        case "bcc":
                                _mMailMessage.Bcc.Add(new MailAddress(strEmailAddress));
                        case "replyTo":
                                _mMailMessage.ReplyToList.Add(new MailAddress(strEmailAddress));
                catch (FormatException e)
                    ErrorRecord er = new(e, "FormatException", ErrorCategory.InvalidType, null);
                // Set the sender address of the mail message
                _mMailMessage.From = new MailAddress(From);
                ErrorRecord er = new(e, "FormatException", ErrorCategory.InvalidType, From);
            // Set the recipient address of the mail message
            AddAddressesToMailMessage(To, "to");
            // Set the BCC address of the mail message
            if (Bcc != null)
                AddAddressesToMailMessage(Bcc, "bcc");
            // Set the CC address of the mail message
            if (Cc != null)
                AddAddressesToMailMessage(Cc, "cc");
            // Set the Reply-To address of the mail message
            if (ReplyTo != null)
                AddAddressesToMailMessage(ReplyTo, "replyTo");
            // Set the delivery notification
            _mMailMessage.DeliveryNotificationOptions = DeliveryNotificationOption;
            // Set the subject of the mail message
            _mMailMessage.Subject = Subject;
            // Set the body of the mail message
            _mMailMessage.Body = Body;
            // Set the subject and body encoding
            _mMailMessage.SubjectEncoding = Encoding;
            _mMailMessage.BodyEncoding = Encoding;
            // Set the format of the mail message body as HTML
            _mMailMessage.IsBodyHtml = BodyAsHtml;
            // Set the priority of the mail message to normal
            _mMailMessage.Priority = Priority;
            // Get the PowerShell environment variable
            // globalEmailServer might be null if it is deleted by: PS> del variable:PSEmailServer
            PSVariable globalEmailServer = SessionState.Internal.GetVariable(SpecialVariables.PSEmailServer);
            if (SmtpServer == null && globalEmailServer != null)
                SmtpServer = Convert.ToString(globalEmailServer.Value, CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(SmtpServer))
                ErrorRecord er = new(new InvalidOperationException(SendMailMessageStrings.HostNameValue), null, ErrorCategory.InvalidArgument, null);
                this.ThrowTerminatingError(er);
            if (Port == 0)
                _mSmtpClient = new SmtpClient(SmtpServer);
                _mSmtpClient = new SmtpClient(SmtpServer, Port);
            if (UseSsl)
                _mSmtpClient.EnableSsl = true;
                _mSmtpClient.UseDefaultCredentials = false;
                _mSmtpClient.Credentials = Credential.GetNetworkCredential();
            else if (!UseSsl)
                _mSmtpClient.UseDefaultCredentials = true;
        /// ProcessRecord override.
            // Add the attachments
            if (Attachments != null)
                string filepath = string.Empty;
                foreach (string attachFile in Attachments)
                        filepath = PathUtils.ResolveFilePath(attachFile, this);
                        // NOTE: This will throw
                        PathUtils.ReportFileOpenFailure(this, filepath, e);
                    Attachment mailAttachment = new(filepath);
                    _mMailMessage.Attachments.Add(mailAttachment);
        /// EndProcessing override.
                // Send the mail message
                _mSmtpClient.Send(_mMailMessage);
            catch (SmtpFailedRecipientsException ex)
                ErrorRecord er = new(ex, "SmtpFailedRecipientsException", ErrorCategory.InvalidOperation, _mSmtpClient);
            catch (SmtpException ex)
                    ErrorRecord er = new(new SmtpException(ex.InnerException.Message), "SmtpException", ErrorCategory.InvalidOperation, _mSmtpClient);
                    ErrorRecord er = new(ex, "SmtpException", ErrorCategory.InvalidOperation, _mSmtpClient);
                ErrorRecord er = new(ex, "InvalidOperationException", ErrorCategory.InvalidOperation, _mSmtpClient);
            catch (System.Security.Authentication.AuthenticationException ex)
                ErrorRecord er = new(ex, "AuthenticationException", ErrorCategory.InvalidOperation, _mSmtpClient);
                _mSmtpClient.Dispose();
                // If we don't dispose the attachments, the sender can't modify or use the files sent.
                _mMailMessage.Attachments.Dispose();

        /// Prompt for credentials.
        /// In future, when we have Credential object from the security team,
        /// this function will be modified to prompt using secure-path
        /// if so configured.
        /// <param name="userName">Name of the user whose creds are to be prompted for. If set to null or empty string, the function will prompt for user name first.</param>
        /// <param name="targetName">Name of the target for which creds are being collected.</param>
        /// <param name="message">Message to be displayed.</param>
        /// <param name="caption">Caption for the message.</param>
        /// <returns>PSCredential object.</returns>
        public override PSCredential PromptForCredential(
            string userName,
            string targetName)
            return PromptForCredential(caption,
                                         userName,
                                         targetName,
                                         PSCredentialTypes.Default,
                                         PSCredentialUIOptions.Default);
        /// <param name="allowedCredentialTypes">What type of creds can be supplied by the user.</param>
        /// <param name="options">Options that control the cred gathering UI behavior.</param>
        /// <returns>PSCredential object, or null if input was cancelled (or if reading from stdin and stdin at EOF).</returns>
            string targetName,
            PSCredentialTypes allowedCredentialTypes,
            PSCredentialUIOptions options)
            PSCredential cred = null;
            SecureString password = null;
            string userPrompt = null;
            string passwordPrompt = null;
            if (string.IsNullOrEmpty(userName))
                userPrompt = ConsoleHostUserInterfaceSecurityResources.PromptForCredential_User;
                // need to prompt for user name first
                    WriteToConsole(userPrompt, true);
                    userName = ReadLine();
                    if (userName == null)
                while (userName.Length == 0);
            passwordPrompt = StringUtil.Format(ConsoleHostUserInterfaceSecurityResources.PromptForCredential_Password, userName
            if (!InternalTestHooks.NoPromptForPassword)
                WriteToConsole(passwordPrompt, transcribeResult: true);
                password = ReadLineAsSecureString();
                if (password == null)
                password = new SecureString();
            if (!string.IsNullOrEmpty(targetName))
                userName = StringUtil.Format("{0}\\{1}", targetName, userName);
            cred = new PSCredential(userName, password);
            return cred;

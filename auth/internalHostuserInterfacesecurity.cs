    class InternalHostUserInterface : PSHostUserInterface
        PSCredential
        PromptForCredential
            string targetName
            return PromptForCredential(caption, message, userName,
            PSCredential result = null;
                result = _externalUI.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);

    /// should accept credentials.
    public sealed class CredentialAttribute : ArgumentTransformationAttribute
        /// Transforms the input data to an PSCredential.
        /// If Null, the transformation prompts for both Username and Password
        /// If a string, the transformation uses the input for a username, and prompts
        ///    for a Password
        /// If already an PSCredential, the transform does nothing.
        /// <returns>An PSCredential object representing the inputData.</returns>
            bool shouldPrompt = false;
            if ((engineIntrinsics == null) ||
               (engineIntrinsics.Host == null) ||
               (engineIntrinsics.Host.UI == null))
                shouldPrompt = true;
                // Try to coerce the input as an PSCredential
                cred = LanguagePrimitives.FromObjectAs<PSCredential>(inputData);
                // Try to coerce the username from the string
                if (cred == null)
                    userName = LanguagePrimitives.FromObjectAs<string>(inputData);
                    // If we couldn't get the username (as a string,)
                    // throw an exception
                        throw new PSArgumentException("userName");
            if (shouldPrompt)
                string caption = null;
                caption = CredentialAttributeStrings.CredentialAttribute_Prompt_Caption;
                prompt = CredentialAttributeStrings.CredentialAttribute_Prompt;
                cred = engineIntrinsics.Host.UI.PromptForCredential(
                           prompt,

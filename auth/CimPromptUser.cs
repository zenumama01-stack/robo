    /// Prompt user the message coming from provider.
    /// At the same time <see cref="CimPromptUser"/> class will prepare the
    /// message for -whatif parameter, while the message represents
    /// what will happen if execute the operation, but not do the operation.
    /// For example, Remove-CimInstance, the whatif message will like,
    /// "CIM Instance: Win32_Process@{Key=1} will be deleted."
    internal sealed class CimPromptUser : CimSyncAction
        /// Initializes a new instance of the <see cref="CimPromptUser"/> class.
        public CimPromptUser(string message,
            CimPromptType prompt)
            this.Message = message;
            this.prompt = prompt;
        /// Prompt user with the given message and prepared whatif message.
        public override void Execute(CmdletOperationBase cmdlet)
            ValidationHelper.ValidateNoNullArgument(cmdlet, "cmdlet");
            bool yestoall = false;
            bool notoall = false;
            bool result = false;
            switch (this.prompt)
                case CimPromptType.Critical:
                    // NOTES: prepare the whatif message and caption
                        result = cmdlet.ShouldContinue(Message, "caption", ref yestoall, ref notoall);
                        if (yestoall)
                            this.responseType = CimResponseType.YesToAll;
                        else if (notoall)
                            this.responseType = CimResponseType.NoToAll;
                        else if (result)
                            this.responseType = CimResponseType.Yes;
                        else if (!result)
                            this.responseType = CimResponseType.No;
                    catch
                        throw;
                        // unblocking the waiting thread
                        this.OnComplete();
                case CimPromptType.Normal:
                        result = cmdlet.ShouldProcess(Message);
        /// Prompt message.
        public string Message { get; }
        /// Prompt type -Normal or Critical.
        private readonly CimPromptType prompt;

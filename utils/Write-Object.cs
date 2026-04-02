    #region WriteOutputCommand
    /// This class implements Write-Output command.
    [Cmdlet(VerbsCommunications.Write, "Output", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097117", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteOutputCommand : PSCmdlet
        /// Holds the list of objects to be written.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
        /// Prevents Write-Output from unravelling collections passed to the InputObject parameter.
        public SwitchParameter NoEnumerate { get; set; }
        /// This method implements the ProcessRecord method for Write-output command.
            WriteObject(InputObject, !NoEnumerate.IsPresent);

    /// Writes the PSHost object to the success stream.
    [Cmdlet(VerbsCommon.Get, "Host", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097110", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(System.Management.Automation.Host.PSHost))]
    class GetHostCommand : PSCmdlet
        /// See base class.
            WriteObject(this.Host);

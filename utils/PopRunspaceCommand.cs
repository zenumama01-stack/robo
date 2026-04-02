    /// Exit-PSSession cmdlet.
    [Cmdlet(VerbsCommon.Exit, "PSSession", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096787")]
    public class ExitPSSessionCommand : PSRemotingCmdlet
            // Pop it off the local host.
            IHostSupportsInteractiveSession host = this.Host as IHostSupportsInteractiveSession;
                        new ArgumentException(GetMessage(RemotingErrorIdStrings.HostDoesNotSupportPushRunspace)),
                        nameof(PSRemotingErrorId.HostDoesNotSupportPushRunspace),

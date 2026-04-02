    /// Returns the thread's current UI culture.
    [Cmdlet(VerbsCommon.Get, "UICulture", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096613")]
    public sealed class GetUICultureCommand : PSCmdlet
        /// Output the current UI Culture info object.
            WriteObject(Host.CurrentUICulture);

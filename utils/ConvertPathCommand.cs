    /// A command to convert a drive qualified or provider qualified path to
    /// a provider internal path.
    [Cmdlet(VerbsData.Convert, "Path", DefaultParameterSetName = "Path", SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096588", RemotingCapability = RemotingCapability.None)]
    public class ConvertPathCommand : CoreCommandBase
                return _paths;
                _paths = value;
            get => base.Force;
            set => base.Force = value;
        /// The path(s) to the item(s) to convert.
        private string[] _paths;
        /// Converts a drive qualified or provider qualified path to a provider
        /// internal path.
                    Collection<string> results =
                        SessionState.Path.GetResolvedProviderPathFromPSPath(
                                CmdletProviderContext,
                                out provider);
                    WriteObject(results, true);

    /// This class implements `Get-SecureRandom` cmdlet.
    [Cmdlet(VerbsCommon.Get, "SecureRandom", DefaultParameterSetName = GetRandomCommandBase.RandomNumberParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2235055", RemotingCapability = RemotingCapability.None)]
    public sealed class GetSecureRandomCommand : GetRandomCommandBase
        // nothing unique from base class

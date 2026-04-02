    /// This class implements `Get-Random` cmdlet.
    [Cmdlet(VerbsCommon.Get, "Random", DefaultParameterSetName = GetRandomCommandBase.RandomNumberParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097016", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(int), typeof(long), typeof(double))]
    public sealed class GetRandomCommand : GetRandomCommandBase
        /// Seed used to reinitialize random numbers generator.
        public int? SetSeed { get; set; }
        /// This method implements the BeginProcessing method for get-random command.
            if (SetSeed.HasValue)
                Generator = new PolymorphicRandomNumberGenerator(SetSeed.Value);

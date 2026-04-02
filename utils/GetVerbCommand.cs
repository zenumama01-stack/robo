using static System.Management.Automation.Verbs;
    /// Implementation of the Get Verb Command.
    [Cmdlet(VerbsCommon.Get, "Verb", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097026")]
    [OutputType(typeof(VerbInfo))]
    public class GetVerbCommand : Cmdlet
        /// Optional Verb filter.
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public string[] Verb
            get; set;
        /// Optional Group filter.
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 1)]
        [ValidateSet("Common", "Communications", "Data", "Diagnostic", "Lifecycle", "Other", "Security")]
        public string[] Group
        /// Returns a list of verbs.
            foreach (VerbInfo verb in FilterByVerbsAndGroups(Verb, Group))
                WriteObject(verb);

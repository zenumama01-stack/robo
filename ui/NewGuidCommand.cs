    /// The implementation of the "new-guid" cmdlet.
    [Cmdlet(VerbsCommon.New, "Guid", DefaultParameterSetName = "Default", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2097130")]
    [OutputType(typeof(Guid))]
    public class NewGuidCommand : PSCmdlet
        /// Gets or sets a value indicating that the cmdlet should return a Guid structure whose value is all zeros.
        [Parameter(ParameterSetName = "Empty")]
        public SwitchParameter Empty { get; set; }
        /// Gets or sets the value to be converted to a Guid.
        [Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public string InputObject { get; set; }
        /// Returns a Guid.
            Guid? guid = null;
            if (ParameterSetName is "InputObject")
                    guid = new(InputObject);
                    ErrorRecord error = new(ex, "StringNotRecognizedAsGuid", ErrorCategory.InvalidArgument, null);
                guid = Empty.ToBool() ? Guid.Empty : Guid.NewGuid();
            WriteObject(guid);

    /// Implementation for the Format-List command.
    [Cmdlet(VerbsCommon.Format, "List", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096928")]
    [OutputType(typeof(FormatStartData), typeof(FormatEntryData), typeof(FormatEndData), typeof(GroupStartData), typeof(GroupEndData))]
    public class FormatListCommand : OuterFormatTableAndListBase
        /// Initializes a new instance of the <see cref="FormatListCommand"/> class
        /// and sets the inner command.
        public FormatListCommand()
            this.implementation = new InnerFormatShapeCommand(FormatShape.List);

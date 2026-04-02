    /// Implementation for the Format-Table command.
    [Cmdlet(VerbsCommon.Format, "Table", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096703")]
    public class FormatTableCommand : OuterFormatTableBase
        /// Initializes a new instance of the <see cref="FormatTableCommand"/> class
        public FormatTableCommand()
            this.implementation = new InnerFormatShapeCommand(FormatShape.Table);

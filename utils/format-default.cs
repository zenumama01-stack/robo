    /// Implementation for the format-default command.
    [Cmdlet(VerbsCommon.Format, "Default")]
    public class FormatDefaultCommand : FrontEndCommandBase
        /// Constructor to set the inner command.
        public FormatDefaultCommand()
            this.implementation = new InnerFormatShapeCommand(FormatShape.Undefined);

    /// Implementation for the out-printer command.
    [Cmdlet(VerbsData.Out, "Printer", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2109553")]
    public class OutPrinterCommand : FrontEndCommandBase
        /// Initializes a new instance of the <see cref="OutPrinterCommand"/> class
        public OutPrinterCommand()
        /// Optional name of the printer to print to.
        /// The alias allows "lp -P printer".
        [Alias("PrinterName")]
            get { return _printerName; }
            set { _printerName = value; }
        private string _printerName;
        /// One-time initialization: acquire a screen host interface by creating one on top of a memory buffer.
            PrinterLineOutput printOutput = new(_printerName);
            return (LineOutput)printOutput;

    /// WriteHost cmdlet.
    [Cmdlet(VerbsCommunications.Write, "Host", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097137", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteHostCommand : ConsoleColorCmdlet
        /// Object to be output.
        [Parameter(Position = 0, ValueFromRemainingArguments = true, ValueFromPipeline = true)]
        public object Object { get; set; }
        /// False to add a newline to the end of the output string, true if not.
        public SwitchParameter NoNewline
                return _notAppendNewline;
                _notAppendNewline = value;
        /// Gets and sets the separator to print between objects.
        public object Separator { get; set; } = " ";
        // Cmdlet Overrides
        private string ProcessObject(object o)
            if (o != null)
                if (o is string s)
                    // strings are IEnumerable, so we special case them
                    if (s.Length > 0)
                else if (o is XmlNode xmlNode)
                    return xmlNode.Name;
                else if (o is IEnumerable enumerable)
                    // unroll enumerables, including arrays.
                    bool printSeparator = false;
                    foreach (object element in enumerable)
                        if (printSeparator && Separator != null)
                            result.Append(Separator.ToString());
                        result.Append(ProcessObject(element));
                        printSeparator = true;
                    s = o.ToString();
        /// Outputs the object to the host console, with optional newline.
            string result = ProcessObject(Object) ?? string.Empty;
            HostInformationMessage informationMessage = new();
            informationMessage.Message = result;
            informationMessage.NoNewLine = NoNewline.IsPresent;
                informationMessage.ForegroundColor = ForegroundColor;
                informationMessage.BackgroundColor = BackgroundColor;
            catch (System.Management.Automation.Host.HostException)
                // Expected if the host is not interactive, or doesn't have Foreground / Background
                // colours.
            this.WriteInformation(informationMessage, new string[] { "PSHOST" });
        private bool _notAppendNewline = false;

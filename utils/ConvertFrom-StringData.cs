    /// Class comment.
    [Cmdlet(VerbsData.ConvertFrom, "StringData", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096602", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(Hashtable))]
    public sealed class ConvertFromStringDataCommand : PSCmdlet
        private string _stringData;
        /// The list of properties to display.
        /// These take the form of an PSPropertyExpression.
        public string StringData
                return _stringData;
                _stringData = value;
        /// Gets or sets the delimiter.
        public char Delimiter { get; set; } = '=';
            Hashtable result = new(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(_stringData))
            string[] lines = _stringData.Split('\n', StringSplitOptions.TrimEntries);
                if (string.IsNullOrEmpty(line) || line[0] == '#')
                int index = line.IndexOf(Delimiter);
                if (index <= 0)
                    throw PSTraceSource.NewInvalidOperationException(
                        ConvertFromStringData.InvalidDataLine,
                        line);
                string name = line.Substring(0, index);
                name = name.Trim();
                if (result.ContainsKey(name))
                        ConvertFromStringData.DataItemAlreadyDefined,
                        line,
                        name);
                string value = line.Substring(index + 1);
                value = value.Trim();
                value = Regex.Unescape(value);
                result.Add(name, value);

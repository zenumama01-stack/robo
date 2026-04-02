    /// This class is used to parse CSV text.
    internal sealed class CSVHelper
        internal CSVHelper(char delimiter)
            Delimiter = delimiter;
        /// Gets or sets the delimiter that separates the values.
        internal char Delimiter { get; } = ',';
        /// Parse a CSV string.
        /// <param name="csv">
        /// String to be parsed.
        internal Collection<string> ParseCsv(string csv)
            Collection<string> result = new();
            string tempString = string.Empty;
            csv = csv.Trim();
            if (csv.Length == 0 || csv[0] == '#')
            bool inQuote = false;
            for (int i = 0; i < csv.Length; i++)
                char c = csv[i];
                if (c == Delimiter)
                    if (!inQuote)
                        result.Add(tempString);
                        tempString = string.Empty;
                        tempString += c;
                            if (inQuote)
                                // If we are at the end of the string or the end of the segment, create a new value
                                // Otherwise we have an error
                                if (i == csv.Length - 1)
                                    inQuote = false;
                                if (csv[i + 1] == Delimiter)
                                else if (csv[i + 1] == '"')
                                    tempString += '"';
                                inQuote = true;
            if (tempString.Length > 0)

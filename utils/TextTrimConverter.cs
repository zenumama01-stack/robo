    /// Removes whitespace at beginning and end of a string.
    public class TextTrimConverter : IValueConverter
        /// Creates a new TextTrimConverter. By default, both conversion directions are trimmed.
        public TextTrimConverter()
        /// Trims excess whitespace from the given string.
        /// <param name="value">Original string.</param>
        /// <returns>The trimmed string.</returns>
            return TrimValue(value);
        private static object TrimValue(object value)
            string strValue = value as string;
            if (strValue == null)
            return strValue.Trim();
        /// Trims extra whitespace from the given string during backward conversion.

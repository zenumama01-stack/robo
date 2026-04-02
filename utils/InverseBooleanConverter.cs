    /// Takes a bool value and returns the inverse.
    public class InverseBooleanConverter : IValueConverter
        /// Converts a boolean value to be it's inverse.
        /// <param name="value">The source value.</param>
        /// <returns>The inverted boolean value.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            ArgumentNullException.ThrowIfNull(value);
            var boolValue = (bool)value;
            return !boolValue;
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)

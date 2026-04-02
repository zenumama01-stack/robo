    /// Takes a value and returns the largest value which is a integral amount of the second value.
    public class IntegralConverter : IMultiValueConverter
        /// <param name="values">
        /// The first value is the source.  The second is the factor.
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The padding to subtract from the first value.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// The integral value.
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length != 2)
                throw new ArgumentException("Two values expected", "values");
            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;
            var source = (double)values[0];
            var factor = (double)values[1];
            double padding = 0;
                padding = double.Parse((string)parameter, CultureInfo.InvariantCulture);
            var newSource = source - padding;
            if (newSource < factor)
                return source;
            var remainder = newSource % factor;
            var result = newSource - remainder;
        /// This method is not used.
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetTypes">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <returns>The parameter is not used.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            throw new NotImplementedException();

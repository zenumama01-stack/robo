    /// Converter from ViewGroup to group title string.
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class ViewGroupToStringConverter : IValueConverter
        /// Convert each ViewGroup into its name and its count.
        /// <param name="value">Value to be converted.</param>
        /// <param name="targetType">Type to convert the value to.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">Conversion culture.</param>
        /// <returns>The converted string.</returns>
            CollectionViewGroup cvg = value as CollectionViewGroup;
            if (cvg == null)
                throw new ArgumentException("value must be of type CollectionViewGroup", "value");
            string name = (!string.IsNullOrEmpty(cvg.Name.ToString())) ? cvg.Name.ToString() : UICultureResources.GroupTitleNone;
            string display = string.Create(CultureInfo.CurrentCulture, $"{name} ({cvg.ItemCount})");
            return display;
        /// ConvertBack is not supported.
        /// <returns>This method is not supported.</returns>
        /// <exception cref="NotSupportedException">when calling the method.</exception>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
            // I can't think of nothing that could be added to the exception message
            // that would be of further help

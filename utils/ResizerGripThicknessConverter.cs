    /// A converter which creates the proper thickness for the content of the Resizer, depending on the grip visual size
    /// and grip position.
    /// The first value needs to be a double which is the visible grip size.
    /// The second value needs to the be ResizeGripLocation value used.
    public class ResizerGripThicknessConverter : IMultiValueConverter
        /// Creates an instance of ResizerGripThicknessConverter.
        public ResizerGripThicknessConverter()
        /// <param name="values">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <returns>A converted value. If the method returns nullNothingnullptra null reference (Nothing in Visual Basic), the valid null value is used.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            if (object.ReferenceEquals(values[0], DependencyProperty.UnsetValue) ||
                object.ReferenceEquals(values[1], DependencyProperty.UnsetValue))
            var resizerVisibleGripWidth = (double)values[0];
            var gripLocation = (ResizeGripLocation)values[1];
            return Resizer.CreateGripThickness(resizerVisibleGripWidth, gripLocation);
        /// <param name="targetTypes">The type to convert to.</param>
        /// <returns>A converted values. If the method returns nullNothingnullptra null reference (Nothing in Visual Basic), the valid null value is used.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)

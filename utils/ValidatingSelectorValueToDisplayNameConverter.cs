    /// The ValidatingSelectorValueToDisplayNameConverterTakes class is responsible for returning a display
    /// friendly name for the ValidatingSelectorValue class.
    public class ValidatingSelectorValueToDisplayNameConverter : IMultiValueConverter
        /// Takes in a value and a converter and runs the converter on the value returning
        /// a display friendly name.
        /// The first parameter is the value to get the display name for.
        /// The second parameter is the converter.
        /// Type of string.
        /// Returns a display friendly name for the first value.
            // NOTE : null values are ok.
            object input = values[0];
            IValueConverter converter = values[1] as IValueConverter;
            if (converter == null)
                throw new ArgumentException("Second value should be a IValueConverter", "values");
            if (targetType != typeof(string))
                throw new ArgumentException("targetType should be of type string", "targetType");
            return converter.Convert(input, targetType, parameter, culture);
        /// <param name="targetTypes">

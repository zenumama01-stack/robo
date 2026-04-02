    /// The ValidatingValueToGenericParameterTypeConverter class is responsible for
    /// converting a <see cref="ValidatingValue{T}"/> to its generic parameter T.
    public class ValidatingValueToGenericParameterTypeConverter : IValueConverter
        /// Gets an instance of the ValidatingValueToGenericParameterTypeConverter class.
        public static ValidatingValueToGenericParameterTypeConverter Instance
                return new ValidatingValueToGenericParameterTypeConverter();
        /// Converts a <see cref="ValidatingValue{T}" /> to its generic parameter T.
        /// The <see cref="ValidatingValue{T}"/> to convert.
        /// The type of value.
                return typeof(string);

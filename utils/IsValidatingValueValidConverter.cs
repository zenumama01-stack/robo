    /// The IsValidatingValueValidConverter is responsible for determining if
    /// a ValidatingValueBase object is valid.
    public class IsValidatingValueValidConverter : IValueConverter
        /// Determines if ValidatingValueBase.Error indicates
        /// if the object is valid.
        /// The Error string to check.
        /// Returns true if value is null or empty, false otherwise.
            string error = (string)value;
            return string.IsNullOrEmpty(error);

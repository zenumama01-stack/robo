    /// The IsNotNullConverter is responsible for converting a value into
    /// a boolean indicting whether the value is not null.
    public class IsNotNullConverter : IValueConverter
        #region IValueConverter Members
        /// Determines if value is not null.
        /// <param name="value">The object to check.</param>
        /// <returns>Returns true if value is not null, false otherwise.</returns>
            return value != null;
            throw new NotSupportedException();

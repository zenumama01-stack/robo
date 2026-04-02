    /// Converts the value of the single <see cref="Binding"/> in a
    /// <see cref="MultiBinding"/> to a string,
    /// and returns that string if not null/empty,
    /// otherwise returns DefaultValue.
    /// The <see cref="MultiBinding"/> must have exactly one
    /// <see cref="Binding"/>.
    /// The problem solved by this <see cref="IMultiValueConverter"/>
    /// is that for an ordinary <see cref="Binding"/> which is bound to
    /// "Path=PropertyA.PropertyB", the Converter is not called if the value
    /// of PropertyA was null (and therefore PropertyB could not be accessed).
    /// By contrast, the converter for an <see cref="IMultiValueConverter"/>
    /// will be called even if any or all of the bindings fail to evaluate
    /// down to the last property.
    /// Note that the <see cref="MultiBinding"/> which uses this
    /// <see cref="IMultiValueConverter"/> must have exactly one
    public class DefaultStringConverter : IMultiValueConverter
        /// Gets or sets default string returned by the converter
        /// if the value is null/empty.
        public string DefaultValue
        /// Converts the value of the single <see cref="Binding"/> in the
        /// <see cref="IMultiValueConverter"/> to a string,
        /// Must contain exactly one value, of any type.
        /// A string, either the value of the first <see cref="Binding"/>
        /// converted to string, or DefaultValue.
            if (values.Length != 1)
                throw new ArgumentNullException("values");
            string val = values[0] as string;
            if (!string.IsNullOrEmpty(val))
                return val;
            return this.DefaultValue;
        /// Skip ConvertBack binding.
        /// <returns>Binding.DoNothing blocks ConvertBack binding.</returns>
            return new object[1] { Binding.DoNothing };

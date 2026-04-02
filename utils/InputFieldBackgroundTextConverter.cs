    /// The InputFieldBackgroundTextConverter is responsible for determining the
    /// correct background text to display for a particular type of data.
    public class InputFieldBackgroundTextConverter : IValueConverter
        private static readonly Type ValidatingValueGenericType = typeof(ValidatingValue<>);
        /// Converts a value of type ValidatingValue of T into a background string
        /// which provides a hint to the end user (e.g. Empty, M/d/yy).
        /// A value of type ValidatingValue.
        /// Returns a background string for value.
            Type inputType = null;
            if (this.IsOfTypeValidatingValue(value))
                inputType = this.GetGenericParameter(value, culture);
            return this.GetBackgroundTextForType(inputType);
        private bool IsOfTypeValidatingValue(object value)
            Debug.Assert(value != null, "not null");
            Type type = value.GetType();
            if (type.IsGenericType == false)
            return type == ValidatingValueGenericType;
        private Type GetGenericParameter(object value, CultureInfo culture)
            Debug.Assert(this.IsOfTypeValidatingValue(value), "not null");
            return value.GetType().GetGenericArguments()[0];
        private object GetBackgroundTextForType(Type inputType)
            if (typeof(DateTime) == inputType)
                return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                // <Empty>
                return XamlLocalizableResources.AutoResXGen_FilterRulePanel_BackgroundText_200;

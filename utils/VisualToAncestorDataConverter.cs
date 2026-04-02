using System.Reflection;
    /// Provides a way to get the <see cref="FrameworkElement.DataContext"/> of a visual ancestor.
    public class VisualToAncestorDataConverter : IValueConverter
        /// Searches ancestors for data of the specified class type.
        /// <param name="value">The visual whose ancestors are searched.</param>
        /// <param name="parameter">The type of the data to find. The type must be a class.</param>
        /// <returns>The data of the specified type; or if not found, <c>null</c>.</returns>
        /// <exception cref="ArgumentException">The specified value is not a class type.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            Type dataType = (Type)parameter;
            if (dataType.IsClass == false)
                throw new ArgumentException("The specified value is not a class type.", "parameter");
            DependencyObject obj = (DependencyObject)value;
            MethodInfo findVisualAncestorDataMethod = typeof(WpfHelp).GetMethod("FindVisualAncestorData");
            MethodInfo genericFindVisualAncestorDataMethod = findVisualAncestorDataMethod.MakeGenericMethod(dataType);
            return genericFindVisualAncestorDataMethod.Invoke(null, new object[] { obj });

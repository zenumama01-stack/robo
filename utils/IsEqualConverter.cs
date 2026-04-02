    /// Takes two objects and determines whether they are equal.
    public class IsEqualConverter : IMultiValueConverter
        /// Takes two items and determines whether they are equal.
        /// Two objects of any type.
        /// True if-and-only-if the two objects are equal per Object.Equals().
        /// Null is equal only to null.
            object item1 = values[0];
            object item2 = values[1];
            if (item1 == null)
                return item2 == null;
            if (item2 == null)
            bool equal = item1.Equals(item2);
            return equal;

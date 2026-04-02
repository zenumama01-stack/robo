    #region PSObject Comparer
    /// Keeps the property value of inputObject. Because the value of a non-existing property is null,
    /// isExistingProperty is needed to distinguish whether a property exists and its value is null or
    /// the property does not exist at all.
    internal sealed class ObjectCommandPropertyValue
        private ObjectCommandPropertyValue() { }
        internal ObjectCommandPropertyValue(object propVal)
            PropertyValue = propVal;
            IsExistingProperty = true;
        /// Initializes a new instance of the <see cref="ObjectCommandPropertyValue"/> class.
        /// <param name="propVal">Property Value.</param>
        /// <param name="isCaseSensitive">Indicates if the Property value comparison has to be case sensitive or not.</param>
        /// <param name="cultureInfo">Culture Info of the Property Value.</param>
        internal ObjectCommandPropertyValue(object propVal, bool isCaseSensitive, CultureInfo cultureInfo)
            : this(propVal)
            _caseSensitive = isCaseSensitive;
            this.cultureInfo = cultureInfo;
        internal object PropertyValue { get; }
        internal bool IsExistingProperty { get; }
        /// Indicates if the Property Value comparison has to be Case sensitive or not.
        internal SwitchParameter CaseSensitive
            get { return _caseSensitive; }
        /// Gets the Culture Info of the Property Value.
        internal CultureInfo Culture
                return cultureInfo;
        internal static readonly ObjectCommandPropertyValue NonExistingProperty = new();
        internal static readonly ObjectCommandPropertyValue ExistingNullProperty = new(null);
        private readonly bool _caseSensitive;
        internal CultureInfo cultureInfo = null;
        /// Provides an Equals implementation.
        /// <param name="inputObject">Input Object.</param>
        /// <returns>True if both the objects are same or else returns false.</returns>
        public override bool Equals(object inputObject)
            if (inputObject is not ObjectCommandPropertyValue objectCommandPropertyValueObject)
            object baseObject = PSObject.Base(PropertyValue);
            object inComingbaseObjectPropertyValue = PSObject.Base(objectCommandPropertyValueObject.PropertyValue);
            if (baseObject is IComparable)
                var success = LanguagePrimitives.TryCompare(baseObject, inComingbaseObjectPropertyValue, CaseSensitive, Culture, out int result);
                return success && result == 0;
            if (baseObject == null && inComingbaseObjectPropertyValue == null)
            if (baseObject != null && inComingbaseObjectPropertyValue != null)
                return baseObject.ToString().Equals(inComingbaseObjectPropertyValue.ToString(), StringComparison.OrdinalIgnoreCase);
            // One of the property values being compared is null.
        /// Provides a GetHashCode() implementation.
        /// <returns>Hashcode in the form of an integer.</returns>
        public override int GetHashCode()
            if (PropertyValue == null)
            if (baseObject == null)
                return baseObject.GetHashCode();
                return baseObject.ToString().GetHashCode();
    /// ObjectCommandComparer class.
    internal sealed class ObjectCommandComparer : IComparer
        /// Initializes a new instance of the <see cref="ObjectCommandComparer"/> class.
        /// Constructor that doesn't set any private field.
        /// Necessary because compareTo can compare two objects by calling
        /// ((ICompare)obj1).CompareTo(obj2) without using a key.
        internal ObjectCommandComparer(bool ascending, CultureInfo cultureInfo, bool caseSensitive)
            _ascendingOrder = ascending;
            _cultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
            _caseSensitive = caseSensitive;
        private static bool IsValueNull(object value)
            object val = PSObject.Base(value);
            return (val == null);
        internal int Compare(ObjectCommandPropertyValue first, ObjectCommandPropertyValue second)
            if (first.IsExistingProperty && second.IsExistingProperty)
                return Compare(first.PropertyValue, second.PropertyValue);
            // if first.IsExistingProperty, !second.IsExistingProperty; otherwise the
            // first branch if would return. Regardless of key orders non existing property
            // will be considered greater than others
            if (first.IsExistingProperty)
            // vice versa for the previous branch
            if (second.IsExistingProperty)
            // both are nonexisting
        /// Main method that will compare first and second by their keys considering case and order.
        /// <param name="first">
        /// First object to extract value.
        /// <param name="second">
        /// Second object to extract value.
        /// 0 if they are the same, less than 0 if first is smaller, more than 0 if first is greater.
        public int Compare(object first, object second)
            // This method will never throw exceptions, two null
            // objects are considered the same
            if (IsValueNull(first) && IsValueNull(second))
            if (first is PSObject firstMsh)
                first = firstMsh.BaseObject;
            if (second is PSObject secondMsh)
                second = secondMsh.BaseObject;
            if (!LanguagePrimitives.TryCompare(first, second, !_caseSensitive, _cultureInfo, out int result))
                // Note that this will occur if the objects do not support
                // IComparable.  We fall back to comparing as strings.
                // being here means the first object doesn't support ICompare
                string firstString = PSObject.AsPSObject(first).ToString();
                string secondString = PSObject.AsPSObject(second).ToString();
                result = _cultureInfo.CompareInfo.Compare(firstString, secondString, _caseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase);
            return _ascendingOrder ? result : -result;
        private readonly CultureInfo _cultureInfo = null;
        private readonly bool _ascendingOrder = true;
        private readonly bool _caseSensitive = false;

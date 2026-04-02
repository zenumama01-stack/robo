using MethodCacheEntry = System.Management.Automation.DotNetAdapter.MethodCacheEntry;
    #region public type converters
    /// Defines a base class implemented when you need to customize the type conversion for a target class.
    /// There are two ways of associating the PSTypeConverter with its target class:
    ///     - Through the type configuration file.
    ///     - By applying a TypeConverterAttribute to the target class.
    /// Unlike System.ComponentModel.TypeConverter, PSTypeConverter can be applied to a family of types (like all types derived from System.Enum).
    /// PSTypeConverter has two main differences from TypeConverter:
    ///     - It can be applied to a family of types and not only the one type as in TypeConverter. In order to do that
    /// ConvertFrom and CanConvertFrom receive destinationType to know to which type specifically we are converting sourceValue.
    ///     - ConvertTo and ConvertFrom receive formatProvider and ignoreCase.
    /// Other differences to System.ComponentModel.TypeConverter:
    ///     - There is no ITypeDescriptorContext.
    ///     - This class is abstract
    public abstract class PSTypeConverter
        private static object GetSourceValueAsObject(PSObject sourceValue)
            if (sourceValue == null)
            if (sourceValue.BaseObject is PSCustomObject)
                return sourceValue;
                return PSObject.Base(sourceValue);
        /// Determines if the converter can convert the <paramref name="sourceValue"/> parameter to the <paramref name="destinationType"/> parameter.
        /// <param name="sourceValue">Value supposedly *not* of the types supported by this converted to be converted to the <paramref name="destinationType"/> parameter.</param>
        /// <param name="destinationType">One of the types supported by this converter to which the <paramref name="sourceValue"/> parameter should be converted.</param>
        /// <returns>True if the converter can convert the <paramref name="sourceValue"/> parameter to the <paramref name="destinationType"/> parameter, otherwise false.</returns>
        public abstract bool CanConvertFrom(object sourceValue, Type destinationType);
        public virtual bool CanConvertFrom(PSObject sourceValue, Type destinationType)
            return this.CanConvertFrom(GetSourceValueAsObject(sourceValue), destinationType);
        /// Converts the <paramref name="sourceValue"/> parameter to the <paramref name="destinationType"/> parameter using formatProvider and ignoreCase.
        /// <param name="destinationType">One of the types supported by this converter to which the <paramref name="sourceValue"/> parameter should be converted to.</param>
        /// <param name="formatProvider">The format provider to use like in IFormattable's ToString.</param>
        /// <param name="ignoreCase">True if case should be ignored.</param>
        /// <returns>The <paramref name="sourceValue"/> parameter converted to the <paramref name="destinationType"/> parameter using formatProvider and ignoreCase.</returns>
        /// <exception cref="InvalidCastException">If no conversion was possible.</exception>
        public abstract object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase);
        public virtual object ConvertFrom(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
            return this.ConvertFrom(GetSourceValueAsObject(sourceValue), destinationType, formatProvider, ignoreCase);
        /// Returns true if the converter can convert the <paramref name="sourceValue"/> parameter to the <paramref name="destinationType"/> parameter.
        /// <param name="sourceValue">Value supposedly from one of the types supported by this converter to be converted to the <paramref name="destinationType"/> parameter.</param>
        /// <param name="destinationType">Type to convert the <paramref name="sourceValue"/> parameter, supposedly not one of the types supported by the converter.</param>
        public abstract bool CanConvertTo(object sourceValue, Type destinationType);
        public virtual bool CanConvertTo(PSObject sourceValue, Type destinationType)
            return this.CanConvertTo(GetSourceValueAsObject(sourceValue), destinationType);
        /// <returns>SourceValue converted to the <paramref name="destinationType"/> parameter using formatProvider and ignoreCase.</returns>
        public abstract object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase);
        public virtual object ConvertTo(PSObject sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
            return this.ConvertTo(GetSourceValueAsObject(sourceValue), destinationType, formatProvider, ignoreCase);
    /// Enables a type that only has conversion from string to be converted from all other
    /// types through string.
    /// It is permitted to subclass <see cref="ConvertThroughString"/>
    public class ConvertThroughString : PSTypeConverter
        /// This will return false only if sourceValue is string.
        /// <param name="sourceValue">Value to convert from.</param>
        /// <param name="destinationType">Ignored.</param>
        /// <returns>False only if sourceValue is string.</returns>
        public override bool CanConvertFrom(object sourceValue, Type destinationType)
            // This if avoids infinite recursion.
            if (sourceValue is string)
        /// Converts to destinationType by first converting sourceValue to string
        /// and then converting the result to destinationType.
        /// <param name="sourceValue">The value to convert from.</param>
        /// <param name="destinationType">The type this converter is associated with.</param>
        /// <param name="formatProvider">The IFormatProvider to use.</param>
        /// <returns>SourceValue converted to destinationType.</returns>
        /// <exception cref="PSInvalidCastException">When no conversion was possible.</exception>
        public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
            string sourceAsString = (string)LanguagePrimitives.ConvertTo(sourceValue, typeof(string), formatProvider);
            return LanguagePrimitives.ConvertTo(sourceAsString, destinationType, formatProvider);
        /// Returns false, since this converter is not designed to be used to
        /// convert from the type associated with the converted to other types.
        /// <param name="destinationType">The value to convert from.</param>
        /// <returns>False.</returns>
        public override bool CanConvertTo(object sourceValue, Type destinationType)
        /// Throws NotSupportedException, since this converter is not designed to be used to
        /// <returns>This method does not return a value.</returns>
        /// <exception cref="NotSupportedException">NotSupportedException is always thrown.</exception>
        public override object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
    #endregion public type converters
    /// The ranking of versions for comparison purposes (used in overload resolution.)
    /// A larger value means the conversion is better.
    /// Note that the lower nibble is all ones for named conversion ranks.  This allows for
    /// conversions with rankings in between the named values.  For example, int=>string[]
    /// is value dependent, if the conversion from int=>string succeeds, then an array is
    /// created, otherwise we try some other conversion.  The int=>string[] conversion should
    /// be worse than int=>string, but it is probably better than many other conversions, so
    /// we want it to be only slightly worse than int=>string.
    /// ValueDependent is a flag, but we don't mark the enum as flags because it really isn't
    /// a flags enum.
    /// To make debugging easier, the "in between" conversions are all named, though there are
    /// no references in the code.  They all use the suffix S2A which means "scalar to array".
    internal enum ConversionRank
        None = 0x0000,
        UnrelatedArraysS2A = 0x0007,
        UnrelatedArrays = 0x000F,
        ToStringS2A = 0x0017,
        ToString = 0x001F,
        CustomS2A = 0x0027,
        Custom = 0x002F,
        IConvertibleS2A = 0x0037,
        IConvertible = 0x003F,
        ImplicitCastS2A = 0x0047,
        ImplicitCast = 0x004F,
        ExplicitCastS2A = 0x0057,
        ExplicitCast = 0x005F,
        ConstructorS2A = 0x0067,
        Constructor = 0x006F,
        Create = 0x0073,
        ParseS2A = 0x0077,
        Parse = 0x007F,
        PSObjectS2A = 0x0087,
        PSObject = 0x008F,
        LanguageS2A = 0x0097,
        Language = 0x009F,
        NullToValue = 0x00AF,
        NullToRef = 0x00BF,
        NumericExplicitS2A = 0x00C7,
        NumericExplicit = 0x00CF,
        NumericExplicit1S2A = 0x00D7,
        NumericExplicit1 = 0x00DF,
        NumericStringS2A = 0x00E7,
        NumericString = 0x00EF,
        NumericImplicitS2A = 0x00F7,
        NumericImplicit = 0x00FF,
        AssignableS2A = 0x0107,
        Assignable = 0x010F,
        IdentityS2A = 0x0117,
        StringToCharArray = 0x011A,
        Identity = 0x011F,
        ValueDependent = 0xFFF7,
    /// Defines language support methods.
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Refactoring LanguagePrimitives takes lot of dev/test effort. Since V1 code is already shipped, we tend to exclude this message.")]
    public static class LanguagePrimitives
        internal delegate void MemberNotFoundError(PSObject pso, DictionaryEntry property, Type resultType);
        internal delegate void MemberSetValueError(SetValueException e);
        internal const string OrderedAttribute = "ordered";
        internal const string DoublePrecision = "G15";
        internal const string SinglePrecision = "G7";
        internal static void CreateMemberNotFoundError(PSObject pso, DictionaryEntry property, Type resultType)
            string settableProperties = GetSettableProperties(pso);
            string message = settableProperties == string.Empty
                ? StringUtil.Format(ExtendedTypeSystem.NoSettableProperty, property.Key.ToString(), resultType.FullName)
                : StringUtil.Format(ExtendedTypeSystem.PropertyNotFound, property.Key.ToString(), resultType.FullName, settableProperties);
            typeConversion.WriteLine("Issuing an error message about not being able to create an object from hashtable.");
        internal static void CreateMemberSetValueError(SetValueException e)
            typeConversion.WriteLine("Issuing an error message about not being able to set the properties for an object.");
        static LanguagePrimitives()
            RebuildConversionCache();
            InitializeGetEnumerableCache();
#if !CORECLR // AppDomain Not In CoreCLR
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveHelper;
        internal static void UpdateTypeConvertFromTypeTable(string typeName)
            lock (s_converterCache)
                foreach (var key in s_converterCache.Keys)
                    if (string.Equals(key.to.FullName, typeName, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(key.from.FullName, typeName, StringComparison.OrdinalIgnoreCase))
                        s_converterCache.Remove(key);
                // Note we do not clear possibleTypeConverter even when removing.
                // The conversion cache and the possibleTypeConverter cache are process wide, but
                // the type table used for any specific conversion is runspace specific.
                s_possibleTypeConverter[typeName] = true;
        #region GetEnumerable/GetEnumerator
        /// This is a wrapper class that allows us to use the generic IEnumerable
        /// implementation of an object when we can't use it's non-generic
        /// implementation.
        private sealed class EnumerableTWrapper : IEnumerable
            private readonly object _enumerable;
            private readonly Type _enumerableType;
            private DynamicMethod _getEnumerator;
            internal EnumerableTWrapper(object enumerable, Type enumerableType)
                _enumerable = enumerable;
                _enumerableType = enumerableType;
                CreateGetEnumerator();
            private void CreateGetEnumerator()
                _getEnumerator = new DynamicMethod("GetEnumerator", typeof(object),
                    new Type[] { typeof(object) }, typeof(LanguagePrimitives).Module, true);
                ILGenerator emitter = _getEnumerator.GetILGenerator();
                emitter.Emit(OpCodes.Castclass, _enumerableType);
                MethodInfo methodInfo = _enumerableType.GetMethod("GetEnumerator", Array.Empty<Type>());
                emitter.Emit(OpCodes.Callvirt, methodInfo);
            #region IEnumerable Members
            public IEnumerator GetEnumerator()
                return (IEnumerator)_getEnumerator.Invoke(null, new object[] { _enumerable });
        private static IEnumerable GetEnumerableFromIEnumerableT(object obj)
            foreach (Type i in obj.GetType().GetInterfaces())
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return new EnumerableTWrapper(obj, i);
        private delegate IEnumerable GetEnumerableDelegate(object obj);
        private static readonly Dictionary<Type, GetEnumerableDelegate> s_getEnumerableCache = new Dictionary<Type, GetEnumerableDelegate>(32);
        private static GetEnumerableDelegate GetOrCalculateEnumerable(Type type)
            GetEnumerableDelegate getEnumerable = null;
            lock (s_getEnumerableCache)
                if (!s_getEnumerableCache.TryGetValue(type, out getEnumerable))
                    getEnumerable = CalculateGetEnumerable(type);
                    s_getEnumerableCache.Add(type, getEnumerable);
            return getEnumerable;
        private static void InitializeGetEnumerableCache()
                // PowerShell doesn't treat strings as enumerables so just return null.
                // we also want to return null on common numeric types very quickly
                s_getEnumerableCache.Clear();
                s_getEnumerableCache.Add(typeof(string), LanguagePrimitives.ReturnNullEnumerable);
                s_getEnumerableCache.Add(typeof(int), LanguagePrimitives.ReturnNullEnumerable);
                s_getEnumerableCache.Add(typeof(double), LanguagePrimitives.ReturnNullEnumerable);
        internal static bool IsTypeEnumerable(Type type)
            if (type == null) { return false; }
            GetEnumerableDelegate getEnumerable = GetOrCalculateEnumerable(type);
            return (getEnumerable != LanguagePrimitives.ReturnNullEnumerable);
        /// Returns True if the language considers obj to be IEnumerable.
        /// IEnumerable or IEnumerable-like object
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Justification = "Since V1 code is already shipped, excluding this message.")]
        public static bool IsObjectEnumerable(object obj)
            return IsTypeEnumerable(PSObject.Base(obj)?.GetType());
        /// Retrieves the IEnumerable of obj or null if the language does not consider obj to be IEnumerable.
        public static IEnumerable GetEnumerable(object obj)
            if (obj == null) { return null; }
            GetEnumerableDelegate getEnumerable = GetOrCalculateEnumerable(obj.GetType());
            return getEnumerable(obj);
        private static IEnumerable ReturnNullEnumerable(object obj)
        private static IEnumerable DataTableEnumerable(object obj)
            return (((DataTable)obj).Rows);
        private static IEnumerable TypicalEnumerable(object obj)
            IEnumerable e = (IEnumerable)obj;
                // Some IEnumerable implementations just return null.  Others
                // raise an exception.  Either of these may have a perfectly
                // good generic implementation, so we'll try those if the
                // non-generic is no good.
                if (e.GetEnumerator() == null)
                    return GetEnumerableFromIEnumerableT(obj);
            catch (Exception innerException)
                e = GetEnumerableFromIEnumerableT(obj);
                    "ExceptionInGetEnumerator",
                    ExtendedTypeSystem.EnumerationException,
                    innerException.Message);
        private static GetEnumerableDelegate CalculateGetEnumerable(Type objectType)
            if (typeof(DataTable).IsAssignableFrom(objectType))
                return LanguagePrimitives.DataTableEnumerable;
            // Don't treat IDictionary or XmlNode as enumerable...
            if (typeof(IEnumerable).IsAssignableFrom(objectType)
                && !typeof(IDictionary).IsAssignableFrom(objectType)
                && !typeof(XmlNode).IsAssignableFrom(objectType))
                return LanguagePrimitives.TypicalEnumerable;
            return LanguagePrimitives.ReturnNullEnumerable;
        private static readonly CallSite<Func<CallSite, object, IEnumerator>> s_getEnumeratorSite =
        /// Retrieves the IEnumerator of obj or null if the language does not consider obj as capable of returning an IEnumerator.
        /// <exception cref="ExtendedTypeSystemException">When the act of getting the enumerator throws an exception.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Justification = "Since V1 code is already shipped, excluding this message for backward compatibility reasons.")]
        public static IEnumerator GetEnumerator(object obj)
            var result = s_getEnumeratorSite.Target.Invoke(s_getEnumeratorSite, obj);
            return (result is EnumerableOps.NonEnumerableObjectEnumerator) ? null : result;
        #endregion GetEnumerable/GetEnumerator
        /// This method takes a an arbitrary object and wraps it in a PSDataCollection of PSObject.
        /// This simplifies interacting with the PowerShell workflow activities.
        /// <param name="inputValue"></param>
        public static PSDataCollection<PSObject> GetPSDataCollection(object inputValue)
            PSDataCollection<PSObject> result = new PSDataCollection<PSObject>();
            if (inputValue != null)
                IEnumerator e = GetEnumerator(inputValue);
                        result.Add(e.Current == null ? null : PSObject.AsPSObject(e.Current));
                    result.Add(PSObject.AsPSObject(inputValue));
            result.Complete();
        /// Used to compare two objects for equality converting the second to the type of the first, if required.
        /// <param name="first">First object.</param>
        /// <param name="second">Object to compare first to.</param>
        /// <returns>True if first is equal to the second.</returns>
        public static new bool Equals(object first, object second)
            return Equals(first, second, false, CultureInfo.InvariantCulture);
        /// <param name="ignoreCase">used only if first and second are strings
        /// to specify the type of string comparison </param>
        public static bool Equals(object first, object second, bool ignoreCase)
            return Equals(first, second, ignoreCase, CultureInfo.InvariantCulture);
        /// <param name="formatProvider">the format/culture to be used. If this parameter is null,
        /// CultureInfo.InvariantCulture will be used.
        public static bool Equals(object first, object second, bool ignoreCase, IFormatProvider formatProvider)
            // If both first and second are null it returns true.
            // If one is null and the other is not it returns false.
            // if (first.Equals(second)) it returns true otherwise it goes ahead with type conversion operations.
            // If both first and second are strings it returns (string.Compare(firstString, secondString, ignoreCase) == 0).
            // If second can be converted to the type of the first, it does so and returns first.Equals(secondConverted)
            // Otherwise false is returned
            formatProvider ??= CultureInfo.InvariantCulture;
            if (formatProvider is not CultureInfo culture)
                throw PSTraceSource.NewArgumentException(nameof(formatProvider));
            first = PSObject.Base(first);
            second = PSObject.Base(second);
                return second == null;
            if (second == null)
                return false; // first is not null
            string firstString = first as string;
            string secondString;
                secondString = second as string ?? (string)LanguagePrimitives.ConvertTo(second, typeof(string), culture);
                return (culture.CompareInfo.Compare(firstString, secondString,
                                                    ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) == 0);
            if (first.Equals(second)) return true;
            Type firstType = first.GetType();
            Type secondType = second.GetType();
            int firstIndex = LanguagePrimitives.TypeTableIndex(firstType);
            int secondIndex = LanguagePrimitives.TypeTableIndex(secondType);
            if ((firstIndex != -1) && (secondIndex != -1))
                return LanguagePrimitives.NumericCompare(first, second, firstIndex, secondIndex) == 0;
            if (firstType == typeof(char) && ignoreCase)
                secondString = second as string;
                if (secondString != null && secondString.Length == 1)
                    char firstAsUpper = culture.TextInfo.ToUpper((char)first);
                    char secondAsUpper = culture.TextInfo.ToUpper(secondString[0]);
                    return firstAsUpper.Equals(secondAsUpper);
                else if (secondType == typeof(char))
                    char secondAsUpper = culture.TextInfo.ToUpper((char)second);
                object secondConverted = LanguagePrimitives.ConvertTo(second, firstType, culture);
                return first.Equals(secondConverted);
        /// Helper method for [Try]Compare to determine object ordering with null.
        /// <param name="value">The numeric value to compare to null.</param>
        /// <param name="numberIsRightHandSide">True if the number to compare is on the right hand side if the comparison.</param>
        private static int CompareObjectToNull(object value, bool numberIsRightHandSide)
            var i = numberIsRightHandSide ? -1 : 1;
            // If it's a positive number, including 0, it's greater than null
            // for everything else it's less than zero...
            switch (value)
                case Int16 i16: return Math.Sign(i16) < 0 ? -i : i;
                case Int32 i32: return Math.Sign(i32) < 0 ? -i : i;
                case Int64 i64: return Math.Sign(i64) < 0 ? -i : i;
                case sbyte sby: return Math.Sign(sby) < 0 ? -i : i;
                case float f: return Math.Sign(f) < 0 ? -i : i;
                case double d: return Math.Sign(d) < 0 ? -i : i;
                case decimal de: return Math.Sign(de) < 0 ? -i : i;
                default: return i;
        /// Compare first and second, converting second to the
        /// type of the first, if necessary.
        /// <param name="first">First comparison value.</param>
        /// <param name="second">Second comparison value.</param>
        /// <returns>Less than zero if first is smaller than second, more than
        /// zero if it is greater or zero if they are the same.</returns>
        /// <paramref name="first"/> does not implement IComparable or <paramref name="second"/> cannot be converted
        /// to the type of <paramref name="first"/>.
        public static int Compare(object first, object second)
            return LanguagePrimitives.Compare(first, second, false, CultureInfo.InvariantCulture);
        /// <param name="ignoreCase">Used if both values are strings.</param>
        public static int Compare(object first, object second, bool ignoreCase)
            return LanguagePrimitives.Compare(first, second, ignoreCase, CultureInfo.InvariantCulture);
        /// <param name="formatProvider">Used in type conversions and if both values are strings.</param>
        public static int Compare(object first, object second, bool ignoreCase, IFormatProvider formatProvider)
                return second == null ? 0 : CompareObjectToNull(second, true);
                return CompareObjectToNull(first, false);
            if (first is string firstString)
                string secondString = second as string;
                if (secondString == null)
                        secondString = (string)LanguagePrimitives.ConvertTo(second, typeof(string), culture);
                        throw PSTraceSource.NewArgumentException(nameof(second), ExtendedTypeSystem.ComparisonFailure,
                                                                 first.ToString(), second.ToString(), e.Message);
                return culture.CompareInfo.Compare(firstString, secondString,
                                                   ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
                return LanguagePrimitives.NumericCompare(first, second, firstIndex, secondIndex);
            object secondConverted;
                secondConverted = LanguagePrimitives.ConvertTo(second, firstType, culture);
            if (first is IComparable firstComparable)
                return firstComparable.CompareTo(secondConverted);
            if (first.Equals(second))
            // At this point, we know that they aren't equal but we have no way of
            // knowing which should compare greater than the other so we throw an exception.
            throw PSTraceSource.NewArgumentException(nameof(first), ExtendedTypeSystem.NotIcomparable, first.ToString());
        /// Tries to compare first and second, converting second to the type of the first, if necessary.
        /// If a conversion is needed but fails, false is return.
        /// <param name="result">Less than zero if first is smaller than second, more than
        /// zero if it is greater or zero if they are the same.</param>
        /// <returns>True if the comparison was successful, false otherwise.</returns>
        public static bool TryCompare(object first, object second, out int result)
            return TryCompare(first, second, ignoreCase: false, CultureInfo.InvariantCulture, out result);
        /// <param name="result">Less than zero if first is smaller than second, more than zero if it is greater or zero if they are the same.</param>
        public static bool TryCompare(object first, object second, bool ignoreCase, out int result)
            return TryCompare(first, second, ignoreCase, CultureInfo.InvariantCulture, out result);
        /// <param name="result">Less than zero if first is smaller than second, more than  zero if it is greater or zero if they are the same.</param>
        /// <exception cref="ArgumentException">The parameter <paramref name="formatProvider"/> is not a <see cref="CultureInfo"/>.</exception>
        public static bool TryCompare(object first, object second, bool ignoreCase, IFormatProvider formatProvider, out int result)
            result = 0;
            if (first == null && second == null)
                result = CompareObjectToNull(second, true);
                result = CompareObjectToNull(first, false);
                if (second is not string secondString)
                    if (!TryConvertTo(second, culture, out secondString))
                result = culture.CompareInfo.Compare(firstString, secondString, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
            int firstIndex = TypeTableIndex(firstType);
            int secondIndex = TypeTableIndex(secondType);
            if (firstIndex != -1 && secondIndex != -1)
                result = NumericCompare(first, second, firstIndex, secondIndex);
            if (!TryConvertTo(second, firstType, culture, out object secondConverted))
                result = firstComparable.CompareTo(secondConverted);
            // knowing which should compare greater than the other so we return false.
        /// Returns true if the language considers obj to be true.
        /// <param name="obj">Obj to verify if it is true.</param>
        /// <returns>True if obj is true.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Justification = "Since V1 code is already shipped, excluding this message for backward compatibility reasons")]
        public static bool IsTrue(object obj)
            // null is a valid argument - it converts to false...
            if (obj == null || obj == AutomationNull.Value)
            Type objType = obj.GetType();
            if (objType == typeof(bool))
                return (bool)obj;
            if (objType == typeof(string))
                return IsTrue((string)obj);
            if (LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(objType)))
                IConversionData data = GetConversionData(objType, typeof(bool)) ??
                                       CacheConversion(objType, typeof(bool), CreateNumericToBoolConverter(objType), ConversionRank.Language);
                return (bool)data.Invoke(obj, typeof(bool), false, null, null, null);
            if (objType == typeof(SwitchParameter))
                return ((SwitchParameter)obj).ToBool();
            IList objectArray = obj as IList;
            if (objectArray != null)
                return IsTrue(objectArray);
        internal static bool IsTrue(string s)
            return (s.Length != 0);
        internal static bool IsTrue(IList objectArray)
            switch (objectArray.Count)
                // a zero length array is false, so condition is false
                // if the result is an array of length 1, treat it as a scalar...
                    // A possible implementation would be just
                    // return IsTrue(objectArray[0]);
                    // but since we don't want this to recurse indefinitely
                    // we explicitly check the case where it would recurse
                    // and deal with it.
                    if (PSObject.Base(objectArray[0]) is not IList firstElement)
                        return IsTrue(objectArray[0]);
                    if (firstElement.Count < 1) return false;
                    // the first element is an array with more than zero elements
        /// Internal routine that determines if an object meets any of our criteria for null.
        /// <param name="obj">The object to test.</param>
        /// <returns>True if the object is null.</returns>
        internal static bool IsNull(object obj)
            return (obj == null || obj == AutomationNull.Value);
        /// Auxiliary for the cases where we want a new PSObject or null.
        internal static PSObject AsPSObjectOrNull(object obj)
            return PSObject.AsPSObject(obj);
        internal static int TypeTableIndex(Type type)
            switch (LanguagePrimitives.GetTypeCode(type))
                case TypeCode.Int16: return 0;
                case TypeCode.Int32: return 1;
                case TypeCode.Int64: return 2;
                case TypeCode.UInt16: return 3;
                case TypeCode.UInt32: return 4;
                case TypeCode.UInt64: return 5;
                case TypeCode.SByte: return 6;
                case TypeCode.Byte: return 7;
                case TypeCode.Single: return 8;
                case TypeCode.Double: return 9;
                case TypeCode.Decimal: return 10;
                default: return -1;
        /// Table of the largest safe type to which both types can be converted without exceptions.
        /// This table is used for numeric comparisons.
        /// The 4 entries marked as not used, are explicitly dealt with in NumericCompareDecimal.
        /// NumericCompareDecimal exists because doubles and singles can throw
        /// an exception when converted to decimal.
        /// The order of lines and columns cannot be changed since NumericCompare depends on it.
        internal static readonly Type[][] LargestTypeTable = new Type[][]
            //                                       System.Int16            System.Int32            System.Int64            System.UInt16           System.UInt32           System.UInt64           System.SByte            System.Byte             System.Single           System.Double           System.Decimal
            /* System.Int16   */new Type[] { typeof(System.Int16),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.Double),  typeof(System.Int16),   typeof(System.Int16),   typeof(System.Single),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.Int32   */new Type[] { typeof(System.Int32),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.Double),  typeof(System.Int32),   typeof(System.Int32),   typeof(System.Double),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.Int64   */new Type[] { typeof(System.Int64),   typeof(System.Int64),   typeof(System.Int64),   typeof(System.Int64),   typeof(System.Int64),   typeof(System.Decimal), typeof(System.Int64),   typeof(System.Int64),   typeof(System.Double),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.UInt16  */new Type[] { typeof(System.Int32),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.UInt16),  typeof(System.UInt32),  typeof(System.UInt64),  typeof(System.Int32),   typeof(System.UInt16),  typeof(System.Single),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.UInt32  */new Type[] { typeof(System.Int64),   typeof(System.Int64),   typeof(System.Int64),   typeof(System.UInt32),  typeof(System.UInt32),  typeof(System.UInt64),  typeof(System.Int64),   typeof(System.UInt32),  typeof(System.Double),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.UInt64  */new Type[] { typeof(System.Double),  typeof(System.Double),  typeof(System.Decimal), typeof(System.UInt64),  typeof(System.UInt64),  typeof(System.UInt64),  typeof(System.Double),  typeof(System.UInt64),  typeof(System.Double),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.SByte   */new Type[] { typeof(System.Int16),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.Double),  typeof(System.SByte),   typeof(System.Int16),   typeof(System.Single),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.Byte    */new Type[] { typeof(System.Int16),   typeof(System.Int32),   typeof(System.Int64),   typeof(System.UInt16),  typeof(System.UInt32),  typeof(System.UInt64),  typeof(System.Int16),   typeof(System.Byte),    typeof(System.Single),  typeof(System.Double),  typeof(System.Decimal) },
            /* System.Single  */new Type[] { typeof(System.Single),  typeof(System.Double),  typeof(System.Double),  typeof(System.Single),  typeof(System.Double),  typeof(System.Double),  typeof(System.Single),  typeof(System.Single),  typeof(System.Single),  typeof(System.Double),  null/*not used*/       },
            /* System.Double  */new Type[] { typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  typeof(System.Double),  null/*not used*/       },
            /* System.Decimal */new Type[] { typeof(System.Decimal), typeof(System.Decimal), typeof(System.Decimal), typeof(System.Decimal), typeof(System.Decimal), typeof(System.Decimal), typeof(System.Decimal), typeof(System.Decimal), null/*not used*/,       null/*not used*/,       typeof(System.Decimal) },
        private static int NumericCompareDecimal(decimal decimalNumber, object otherNumber)
            object otherDecimal = null;
                otherDecimal = Convert.ChangeType(otherNumber, typeof(System.Decimal), CultureInfo.InvariantCulture);
                    double wasDecimal = (double)Convert.ChangeType(decimalNumber, typeof(System.Double), CultureInfo.InvariantCulture);
                    double otherDouble = (double)Convert.ChangeType(otherNumber, typeof(System.Double), CultureInfo.InvariantCulture);
                    return ((IComparable)wasDecimal).CompareTo(otherDouble);
                catch (Exception) // We need to catch the generic exception because ChangeType throws unadvertised exceptions
            return ((IComparable)decimalNumber).CompareTo(otherDecimal);
        private static int NumericCompare(object number1, object number2, int index1, int index2)
            // Conversion from single or double to decimal might throw
            // if the double is greater than the decimal's maximum so
            // we special case it in NumericCompareDecimal
            if ((index1 == 10) && ((index2 == 8) || (index2 == 9)))
                return NumericCompareDecimal((decimal)number1, number2);
            if ((index2 == 10) && ((index1 == 8) || (index1 == 9)))
                return -NumericCompareDecimal((decimal)number2, number1);
            Type commonType = LargestTypeTable[index1][index2];
            object number1Converted = Convert.ChangeType(number1, commonType, CultureInfo.InvariantCulture);
            object number2Converted = Convert.ChangeType(number2, commonType, CultureInfo.InvariantCulture);
            return ((IComparable)number1Converted).CompareTo(number2Converted);
        /// Necessary not to return an integer type code for enums.
        internal static TypeCode GetTypeCode(Type type)
            return type.GetTypeCode();
        /// Emulates the "As" C# language primitive, but will unwrap
        /// the PSObject if required.
        /// <typeparam name="T">The type for which to convert</typeparam>
        /// <param name="castObject">The object from which to convert.</param>
        /// <returns>An object of the specified type, if the conversion was successful.  Returns null otherwise.</returns>
        internal static T FromObjectAs<T>(object castObject)
            T returnType = default(T);
            // First, see if we can cast the direct type
            PSObject wrapperObject = castObject as PSObject;
            if (wrapperObject == null)
                    returnType = (T)castObject;
                    returnType = default(T);
            // Then, see if it is an PSObject wrapping the object
                    returnType = (T)wrapperObject.BaseObject;
            return returnType;
        private enum TypeCodeTraits
            None = 0x00,
            SignedInteger = 0x01,
            UnsignedInteger = 0x02,
            Floating = 0x04,
            CimIntrinsicType = 0x08,
            Decimal = 0x10,
            Integer = SignedInteger | UnsignedInteger,
            Numeric = Integer | Floating | Decimal,
        private static readonly TypeCodeTraits[] s_typeCodeTraits = new TypeCodeTraits[]
            /* Empty    =  0 */ TypeCodeTraits.None,
            /* Object   =  1 */ TypeCodeTraits.None,
            /* DBNull   =  2 */ TypeCodeTraits.None,
            /* Boolean  =  3 */ TypeCodeTraits.CimIntrinsicType,
            /* Char     =  4 */ TypeCodeTraits.CimIntrinsicType,
            /* SByte    =  5 */ TypeCodeTraits.SignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* Byte     =  6 */ TypeCodeTraits.UnsignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* Int16    =  7 */ TypeCodeTraits.SignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* UInt16   =  8 */ TypeCodeTraits.UnsignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* Int32    =  9 */ TypeCodeTraits.SignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* UInt32   = 10 */ TypeCodeTraits.UnsignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* Int64    = 11 */ TypeCodeTraits.SignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* UInt64   = 12 */ TypeCodeTraits.UnsignedInteger | TypeCodeTraits.CimIntrinsicType,
            /* Single   = 13 */ TypeCodeTraits.Floating | TypeCodeTraits.CimIntrinsicType,
            /* Double   = 14 */ TypeCodeTraits.Floating | TypeCodeTraits.CimIntrinsicType,
            /* Decimal  = 15 */ TypeCodeTraits.Decimal,
            /* DateTime = 16 */ TypeCodeTraits.None | TypeCodeTraits.CimIntrinsicType,
            /*          = 17 */ TypeCodeTraits.None,
            /* String   = 18 */ TypeCodeTraits.None | TypeCodeTraits.CimIntrinsicType,
        /// Verifies if type is a signed integer.
        /// <param name="typeCode">Type code to check.</param>
        /// <returns>True if type is a signed integer, false otherwise.</returns>
        internal static bool IsSignedInteger(TypeCode typeCode)
            return (s_typeCodeTraits[(int)typeCode] & TypeCodeTraits.SignedInteger) != 0;
        /// Verifies if type is an unsigned integer.
        /// <returns>True if type is an unsigned integer, false otherwise.</returns>
        internal static bool IsUnsignedInteger(TypeCode typeCode)
            return (s_typeCodeTraits[(int)typeCode] & TypeCodeTraits.UnsignedInteger) != 0;
        /// Verifies if type is integer.
        /// <returns>True if type is integer, false otherwise.</returns>
        internal static bool IsInteger(TypeCode typeCode)
            return (s_typeCodeTraits[(int)typeCode] & TypeCodeTraits.Integer) != 0;
        /// Verifies if type is a floating point number.
        /// <returns>True if type is floating point, false otherwise.</returns>
        internal static bool IsFloating(TypeCode typeCode)
            return (s_typeCodeTraits[(int)typeCode] & TypeCodeTraits.Floating) != 0;
        /// Verifies if type is an integer or floating point number.
        /// <returns>True if type is integer or floating point, false otherwise.</returns>
        internal static bool IsNumeric(TypeCode typeCode)
            return (s_typeCodeTraits[(int)typeCode] & TypeCodeTraits.Numeric) != 0;
        /// Verifies if type is a CIM intrinsic type.
        /// <returns>True if type is CIM intrinsic type, false otherwise.</returns>
        internal static bool IsCimIntrinsicScalarType(TypeCode typeCode)
            return (s_typeCodeTraits[(int)typeCode] & TypeCodeTraits.CimIntrinsicType) != 0;
        internal static bool IsCimIntrinsicScalarType(Type type)
            // using type code we can cover all intrinsic types from the table
            // on page 11 of DSP0004, except:
            // - TimeSpan part of "datetime"
            // - <classname> ref
            if (LanguagePrimitives.IsCimIntrinsicScalarType(typeCode) && !type.IsEnum)
        /// Verifies if type is one of the boolean types.
        /// <param name="type">Type to check.</param>
        /// <returns>True if type is one of boolean types, false otherwise.</returns>
        internal static bool IsBooleanType(Type type)
            if (type == typeof(bool) ||
                type == typeof(bool?))
        /// Verifies if type is one of switch parameter types.
        /// <returns>True if type is one of switch parameter types, false otherwise.</returns>
        internal static bool IsSwitchParameterType(Type type)
            if (type == typeof(SwitchParameter) || type == typeof(SwitchParameter?))
        /// Verifies if type is one of boolean or switch parameter types.
        /// <returns>True if type if one of boolean or switch parameter types,
        /// false otherwise.</returns>
        internal static bool IsBoolOrSwitchParameterType(Type type)
            if (IsBooleanType(type) || IsSwitchParameterType(type))
        /// Do the necessary conversions when using property or array assignment to a generic dictionary:
        ///     $dict.Prop = value
        ///     $dict[$Prop] = value
        /// The property typically won't need conversion, but it could.  The value is more likely in
        /// need of conversion.
        /// <param name="dictionary">The dictionary that potentially implement <see cref="IDictionary&lt;TKey,TValue&gt;"/></param>
        /// <param name="key">The object representing the key.</param>
        /// <param name="value">The value to assign.</param>
        internal static void DoConversionsForSetInGenericDictionary(IDictionary dictionary, ref object key, ref object value)
            foreach (Type i in dictionary.GetType().GetInterfaces())
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    // If we get here, we know the target implements IDictionary.  We will assume
                    // that the non-generic implementation of the indexer property just forwards
                    // to the generic version, after checking the types of the key and value.
                    // This assumption holds for System.Collections.Generic.Dictionary<TKey,TValue>.
                    // If we did not make this assumption, we would be forced to generate code
                    // to call the generic indexer directly, somewhat analogous to what we do
                    // in GetEnumeratorFromIEnumeratorT.
                    Type[] genericArguments = i.GetGenericArguments();
                    key = LanguagePrimitives.ConvertTo(key, genericArguments[0], CultureInfo.InvariantCulture);
                    value = LanguagePrimitives.ConvertTo(value, genericArguments[1], CultureInfo.InvariantCulture);
        #region type converter
        internal static readonly PSTraceSource typeConversion = PSTraceSource.GetTracer("TypeConversion", "Traces the type conversion algorithm", false);
        internal static readonly ConversionData<object> NoConversion = new ConversionData<object>(ConvertNoConversion, ConversionRank.None);
        private static TypeConverter GetIntegerSystemConverter(Type type)
            if (type == typeof(Int16))
                return new Int16Converter();
            else if (type == typeof(Int32))
                return new Int32Converter();
            else if (type == typeof(Int64))
                return new Int64Converter();
            else if (type == typeof(UInt16))
                return new UInt16Converter();
            else if (type == typeof(UInt32))
                return new UInt32Converter();
            else if (type == typeof(UInt64))
                return new UInt64Converter();
            else if (type == typeof(byte))
                return new ByteConverter();
            else if (type == typeof(sbyte))
                return new SByteConverter();
        /// backupTypeTable:
        /// Used by Remoting Rehydration Logic. While Deserializing a remote object,
        /// LocalPipeline.ExecutionContextFromTLS() might return null..In which case this
        /// TypeTable will be used to do the conversion.
        internal static object GetConverter(Type type, TypeTable backupTypeTable)
            object typesXmlConverter = null;
            ExecutionContext ecFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (ecFromTLS != null)
                s_tracer.WriteLine("ecFromTLS != null");
                typesXmlConverter = ecFromTLS.TypeTable.GetTypeConverter(type.FullName);
            if ((typesXmlConverter == null) && (backupTypeTable != null))
                s_tracer.WriteLine("Using provided TypeTable to get the type converter");
                typesXmlConverter = backupTypeTable.GetTypeConverter(type.FullName);
            if (typesXmlConverter != null)
                s_tracer.WriteLine("typesXmlConverter != null");
                return typesXmlConverter;
            var typeConverters = type.GetCustomAttributes(typeof(TypeConverterAttribute), false);
            foreach (var typeConverter in typeConverters)
                var attr = (TypeConverterAttribute)typeConverter;
                string assemblyQualifiedtypeName = attr.ConverterTypeName;
                typeConversion.WriteLine("{0}'s TypeConverterAttribute points to {1}.", type, assemblyQualifiedtypeName);
                // The return statement makes sure we only process the first TypeConverterAttribute
                return NewConverterInstance(assemblyQualifiedtypeName);
        private static object NewConverterInstance(string assemblyQualifiedTypeName)
            if (!assemblyQualifiedTypeName.Contains(','))
                typeConversion.WriteLine("Type name \"{0}\" should be assembly qualified.", assemblyQualifiedTypeName);
            Type converterType;
                // Type.GetType() can load an assembly.
                // PowerShell is allowed to load only TPA.
                // Since a type is already loaded we trust to an attribute assigned to the type
                // and can use Type.GetType() without additional checks.
                converterType = Type.GetType(assemblyQualifiedTypeName, throwOnError: true, ignoreCase: false);
                typeConversion.WriteLine("Threw an exception when retrieving the type \"{1}\": \"{2}\".", assemblyQualifiedTypeName, e.Message);
                return Activator.CreateInstance(converterType);
                TargetInvocationException inner = e as TargetInvocationException;
                string message = (inner == null) || (inner.InnerException == null) ? e.Message : inner.InnerException.Message;
                typeConversion.WriteLine("Creating an instance of type \"{0}\" caused an exception to be thrown: \"{1}\"", assemblyQualifiedTypeName, message);
        /// BUGBUG - brucepay Mar. 2013 - I don't think this is general enough for dynamic keywords to support arbitrary target
        /// languages with arbitrary type representations so we may need an extension point here...
        /// Maps a .NET or CIM type name string (e.g. SInt32) to the form expected by PowerShell users, namely "[typename]"
        /// If there is no mapping, then it returns null.
        /// If the string to convert is null or empty then the function returns "[object]" as the default typeless type.
        /// <param name="typeName">The typename string to convert.</param>
        /// <returns>The equivalent PowerShell representation of that type.</returns>
        public static string ConvertTypeNameToPSTypeName(string typeName)
                return "[object]";
            string mappedType;
            if (s_nameMap.TryGetValue(typeName, out mappedType))
                return ('[' + mappedType + ']');
            // Then check dot net types
            Type dotNetType;
            if (TypeResolver.TryResolveType(typeName, out dotNetType))
                // Pass through the canonicalize type name we get back from the type converter...
                return '[' + LanguagePrimitives.ConvertTo<string>(dotNetType) + ']';
            // No mapping is found, return null
        // CIM name string to .NET namestring mapping table
        // (Considered using the MI routines but they didn't do quite the right thing.
        private static readonly Dictionary<string, string> s_nameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "SInt8",          "SByte" },
            { "UInt8",          "Byte" },
            { "SInt16",         "Int16" },
            { "UInt16",         "UInt16" },
            { "SInt32",         "Int32" },
            { "UInt32",         "UInt32" },
            { "SInt64",         "Int64" },
            { "UInt64",         "UInt64" },
            { "Real32",         "Single" },
            { "Real64",         "double" },
            { "Boolean",        "bool" },
            { "String",         "string" },
            { "DateTime",       "DateTime" },
            { "Reference",      "CimInstance" },
            { "Char16",         "char" },
            { "Instance",       "CimInstance" },
            { "BooleanArray",   "bool[]" },
            { "UInt8Array",     "byte[]" },
            { "SInt8Array",     "Sbyte[]" },
            { "UInt16Array",    "UInt16[]" },
            { "SInt16Array",    "Int16[]" },
            { "UInt32Array",    "UInt32[]" },
            { "SInt32Array",    "Int32[]" },
            { "UInt64Array",    "UInt64[]" },
            { "SInt64Array",    "Int64[]" },
            { "Real32Array",    "Single[]" },
            { "Real64Array",    "double[]" },
            { "Char16Array",    "char[]" },
            { "DateTimeArray",  "DateTime[]" },
            { "StringArray",    "string[]" },
            { "ReferenceArray", "CimInstance[]" },
            { "InstanceArray",  "CimInstance[]" },
            { "Unknown",        "UnknownType" },
        #region public type conversion
        /// Converts valueToConvert to resultType.
        /// A null valueToConvert can be converted to :
        ///     string          -   returns ""
        ///     char            -   returns '\0'
        ///     numeric types   -   returns 0 converted to the appropriate type
        ///     boolean         -   returns LanguagePrimitives.IsTrue(null)
        ///     PSObject       -   returns new PSObject())
        ///     array           -   returns an array with null in array[0]
        ///     non value types -   returns null
        /// The following conversions are considered language standard and cannot be customized:
        ///     - from derived to base class            -   returns valueToConvert intact
        ///     - to PSObject                          -   returns PSObject.AsPSObject(valueToConvert)
        ///     - to void                               -   returns AutomationNull.Value
        ///     - from array/IEnumerable to array       -   tries to convert array/IEnumerable elements
        ///     - from object of type X to array of X   -   returns an array with object as its only element
        ///     - to bool                               -   returns LanguagePrimitives.IsTrue(valueToConvert)
        ///     - to string                             -   returns a string representation of the object.
        ///                                                 In the particular case of a number to string,
        ///                                                 the conversion is culture invariant.
        ///     - from IDictionary to Hashtable         -   uses the Hashtable constructor
        ///     - to XmlDocument                        -   creates a new XmlDocument with the
        ///                                                 string representation of valueToConvert
        ///     - from string to char[]                 -   returns ((string)valueToConvert).ToCharArray()
        ///     - from string to RegEx                  -   creates a new RegEx with the string
        ///     - from string to Type                   -   looks up the type in the minishell's assemblies
        ///     - from empty string to numeric          -   returns 0 converted to the appropriate type
        ///     - from string to numeric                -   returns a culture invariant conversion
        ///     - from ScriptBlock to Delegate          -   returns a delegate wrapping that scriptblock.
        ///     - from Integer to Enumeration           -   Uses Enum.ToObject
        ///     - to WMI                                -   Instantiate a WMI instance using
        ///                                                 System.Management.ManagementObject
        ///     - to WMISearcher                        -   returns objects from running WQL query with the
        ///                                                 string representation of valueToConvert. The
        ///                                                 query is run using ManagementObjectSearcher Class.
        ///     - to WMIClass                           -   returns ManagementClass represented by the
        ///                                                 string representation of valueToConvert.
        ///     - to ADSI                               -   returns DirectoryEntry represented by the
        ///     - to ADSISearcher                       -   return DirectorySearcher represented by the
        /// If none of the cases above is true, the following is considered in order:
        ///    1) TypeConverter and PSTypeConverter
        ///    2) the Parse methods if the valueToConvert is a string
        ///    3) Constructors in resultType that take one parameter with type valueToConvert.GetType()
        ///    4) Implicit and explicit cast operators
        ///    5) IConvertible
        ///  If any operation above throws an exception, this exception will be wrapped into a
        ///  PSInvalidCastException and thrown resulting in no further conversion attempt.
        /// <param name="valueToConvert">Value to be converted and returned.</param>
        /// <param name="resultType">Type to convert valueToConvert.</param>
        /// <returns>Converted value.</returns>
        /// <exception cref="ArgumentNullException">If resultType is null.</exception>
        /// <exception cref="PSInvalidCastException">If the conversion failed.</exception>
        public static object ConvertTo(object valueToConvert, Type resultType)
            return ConvertTo(valueToConvert, resultType, true, CultureInfo.InvariantCulture, null);
        /// Converts valueToConvert to resultType possibly considering formatProvider.
        /// <param name="formatProvider">To be used in custom type conversions, to call parse and to call Convert.ChangeType.</param>
        public static object ConvertTo(object valueToConvert, Type resultType, IFormatProvider formatProvider)
            return ConvertTo(valueToConvert, resultType, true, formatProvider, null);
        /// Converts PSObject to resultType.
        /// <param name="resultType">Type to convert psobject.</param>
        /// <param name="recursion">Indicates if inner properties have to be recursively converted.</param>
        /// <param name="ignoreUnknownMembers">Indicates if Unknown members in the psobject have to be ignored if the corresponding members in resultType do not exist.</param>
        public static object ConvertPSObjectToType(PSObject valueToConvert, Type resultType, bool recursion, IFormatProvider formatProvider, bool ignoreUnknownMembers)
            if (valueToConvert != null)
                ConstructorInfo toConstructor = resultType.GetConstructor(Type.EmptyTypes);
                ConvertViaNoArgumentConstructor noArgumentConstructorConverter = new ConvertViaNoArgumentConstructor(toConstructor, resultType);
                return noArgumentConstructorConverter.Convert(PSObject.Base(valueToConvert), resultType, recursion, (PSObject)valueToConvert, formatProvider, null, ignoreUnknownMembers);
        /// Generic convertto that simplifies working with workflow.
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="valueToConvert"></param>
        public static T ConvertTo<T>(object valueToConvert)
            if (valueToConvert is T value)
            return (T)ConvertTo(valueToConvert, typeof(T), true, CultureInfo.InvariantCulture, null);
        /// Sets result to valueToConvert converted to resultType.
        /// This method is a variant of ConvertTo that does not throw exceptions if the conversion fails.
        /// <param name="result">Result of the conversion. This is valid only if the return is true.</param>
        /// <returns>False for conversion failure, true for success.</returns>
        public static bool TryConvertTo<T>(object valueToConvert, out T result)
                result = value;
            return TryConvertTo(valueToConvert, CultureInfo.InvariantCulture, out result);
        /// Sets result to valueToConvert converted to resultType considering formatProvider
        /// for custom conversions, calling the Parse method and calling Convert.ChangeType.
        /// <param name="formatProvider">Governing conversion of types.</param>
        public static bool TryConvertTo<T>(object valueToConvert, IFormatProvider formatProvider, out T result)
            result = default(T);
            if (TryConvertTo(valueToConvert, typeof(T), formatProvider, out object res))
                result = (T)res;
        public static bool TryConvertTo(object valueToConvert, Type resultType, out object result)
            return TryConvertTo(valueToConvert, resultType, CultureInfo.InvariantCulture, out result);
        public static bool TryConvertTo(object valueToConvert, Type resultType, IFormatProvider formatProvider, out object result)
                using (typeConversion.TraceScope("Converting \"{0}\" to \"{1}\".", valueToConvert, resultType))
                    var conversion = FigureConversion(valueToConvert, resultType, out bool debase);
                    if (conversion.Rank == ConversionRank.None)
                    result = conversion.Invoke(
                        debase ? PSObject.Base(valueToConvert) : valueToConvert,
                        resultType,
                        recurse: true,
                        debase ? (PSObject)valueToConvert : null,
                        formatProvider,
                        backupTable: null);
        #endregion public type conversion
        internal class EnumMultipleTypeConverter : EnumSingleTypeConverter
                return EnumSingleTypeConverter.BaseConvertFrom(sourceValue, destinationType, formatProvider, ignoreCase, true);
        internal class EnumSingleTypeConverter : PSTypeConverter
            private sealed class EnumHashEntry
                internal EnumHashEntry(string[] names, Array values, UInt64 allValues, bool hasNegativeValue, bool hasFlagsAttribute)
                    this.names = names;
                    this.values = values;
                    this.allValues = allValues;
                    this.hasNegativeValue = hasNegativeValue;
                    this.hasFlagsAttribute = hasFlagsAttribute;
                internal string[] names;
                internal Array values;
                internal UInt64 allValues;
                internal bool hasNegativeValue;
                internal bool hasFlagsAttribute;
            // This static is thread safe based on the lock in GetEnumHashEntry
            // It can be shared by Runspaces in different MiniShells
            private static readonly Dictionary<Type, EnumHashEntry> s_enumTable = new Dictionary<Type, EnumHashEntry>();
            private const int maxEnumTableSize = 100;
            private static EnumHashEntry GetEnumHashEntry(Type enumType)
                lock (s_enumTable)
                    EnumHashEntry returnValue;
                    if (s_enumTable.TryGetValue(enumType, out returnValue))
                    if (s_enumTable.Count == maxEnumTableSize)
                        s_enumTable.Clear();
                    UInt64 allValues = 0;
                    bool hasNegativeValue = false;
                    Array values = Enum.GetValues(enumType);
                    // Type.GetTypeCode will return the integer type code for enumType
                    if (LanguagePrimitives.IsSignedInteger(enumType.GetTypeCode()))
                        foreach (object value in values)
                            Int64 valueInt64 = Convert.ToInt64(value, CultureInfo.CurrentCulture);
                            // A negative value cannot be flag
                            if (valueInt64 < 0)
                                hasNegativeValue = true;
                            // we know the value is not negative, so this conversion
                            // always succeed
                            allValues |= Convert.ToUInt64(value, CultureInfo.CurrentCulture);
                    // See if the [Flag] attribute is set on this type...
                    // MemberInfo.GetCustomAttributes returns IEnumerable<Attribute> in CoreCLR.
                    bool hasFlagsAttribute = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
                    returnValue = new EnumHashEntry(Enum.GetNames(enumType), values, allValues, hasNegativeValue, hasFlagsAttribute);
                    s_enumTable.Add(enumType, returnValue);
                return sourceValue is string && destinationType.IsEnum;
            /// Checks if the enumValue is defined or not in enumType.
            /// <param name="enumType">Some enumeration.</param>
            /// <param name="enumValue">Supposed to be an integer.</param>
            private static bool IsDefinedEnum(object enumValue, Type enumType)
                bool isDefined;
                    if (enumValue == null)
                        isDefined = false;
                    EnumHashEntry enumHashEntry = EnumSingleTypeConverter.GetEnumHashEntry(enumType);
                    // An enumeration with a negative value should not be treated as flags
                    // so IsValueFlagDefined cannot determine the result, and as far as it knows,
                    // it is defined.
                    if (enumHashEntry.hasNegativeValue)
                        isDefined = true;
                    // Type.GetTypeCode will return the integer type code for enumValue.GetType()
                    if (LanguagePrimitives.IsSignedInteger(enumValue.GetType().GetTypeCode()))
                        Int64 enumValueInt64 = Convert.ToInt64(enumValue, CultureInfo.CurrentCulture);
                        // A negative value cannot be flag, so we return false
                        if (enumValueInt64 < 0)
                    // the if above, guarantees that even if it is an Int64 it is > 0
                    // so the conversion should always work.
                    UInt64 enumValueUInt64 = Convert.ToUInt64(enumValue, CultureInfo.CurrentCulture);
                    if (enumHashEntry.hasFlagsAttribute)
                        // This expression will result in a "1 bit" for bits that are
                        // set in enumValueInt64 but not set in enumHashEntry.allValues,
                        // and a "0 bit" otherwise. Any "bit 1" in the result, indicates this is not defined.
                        isDefined = ((enumValueUInt64 | enumHashEntry.allValues) ^ enumHashEntry.allValues) == 0;
                        // If flags is not set, then see if this value is in the list
                        // of valid values.
                        if (Array.IndexOf(enumHashEntry.values, enumValue) >= 0)
                return isDefined;
            /// Throws if the enumType enumeration has no negative values, but the enumValue is not
            /// defined in enumType.
            /// <param name="errorId">The error id to be used when throwing an exception.</param>
            internal static void ThrowForUndefinedEnum(string errorId, object enumValue, Type enumType)
                ThrowForUndefinedEnum(errorId, enumValue, enumValue, enumType);
            /// <param name="enumValue">Value to validate.</param>
            /// <param name="valueToUseToThrow">Value to use while throwing an exception.</param>
            /// <param name="enumType">The enum type to validate the enumValue with.</param>
            /// <paramref name="valueToUseToThrow"/> is used by those callers who want the exception
            /// to contain a different value than the one that is validated.
            /// This will enable callers to take different forms of input -> convert to enum using
            /// Enum.Object -> then validate using this method.
            internal static void ThrowForUndefinedEnum(string errorId, object enumValue, object valueToUseToThrow, Type enumType)
                if (!IsDefinedEnum(enumValue, enumType))
                    typeConversion.WriteLine("Value {0} is not defined in the Enum {1}.", valueToUseToThrow, enumType);
                    throw new PSInvalidCastException(errorId, null,
                        ExtendedTypeSystem.InvalidCastExceptionEnumerationNoValue,
                        valueToUseToThrow, enumType, EnumSingleTypeConverter.EnumValues(enumType));
            internal static string EnumValues(Type enumType)
                return string.Join(CultureInfo.CurrentUICulture.TextInfo.ListSeparator, enumHashEntry.names);
            /// Returns all names for the provided enum type.
            /// <param name="enumType">The enum type to retrieve names from.</param>
            /// <returns>Array of enum names for the specified type.</returns>
            internal static string[] GetEnumNames(Type enumType)
                => EnumSingleTypeConverter.GetEnumHashEntry(enumType).names;
            /// Returns all values for the provided enum type.
            /// <param name="enumType">The enum type to retrieve values from.</param>
            /// <returns>Array of enum values for the specified type.</returns>
            internal static Array GetEnumValues(Type enumType)
                => EnumSingleTypeConverter.GetEnumHashEntry(enumType).values;
                return EnumSingleTypeConverter.BaseConvertFrom(sourceValue, destinationType, formatProvider, ignoreCase, false);
            protected static object BaseConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase, bool multipleValues)
                Diagnostics.Assert(sourceValue != null, "the type converter has a special case for null source values");
                if (sourceValue is not string sourceValueString)
                    throw new PSInvalidCastException("InvalidCastEnumFromTypeNotAString", null,
                        sourceValue, ObjectToTypeNameString(sourceValue), destinationType);
                Diagnostics.Assert(destinationType.IsEnum, "EnumSingleTypeConverter is only applied to enumerations");
                if (sourceValueString.Length == 0)
                    throw new PSInvalidCastException("InvalidCastEnumFromEmptyString", null,
                sourceValueString = sourceValueString.Trim();
                    throw new PSInvalidCastException("InvalidEnumCastFromEmptyStringAfterTrim", null,
                if (char.IsDigit(sourceValueString[0]) || sourceValueString[0] == '+' || sourceValueString[0] == '-')
                    Type underlyingType = Enum.GetUnderlyingType(destinationType);
                        object result = Enum.ToObject(destinationType, Convert.ChangeType(sourceValueString, underlyingType, formatProvider));
                        ThrowForUndefinedEnum("UndefinedInEnumSingleTypeConverter", result, sourceValueString, destinationType);
                    catch (Exception) // Enum.ToObject and Convert.ChangeType might throw unadvertised exceptions
                        // we still want to try non numeric match
                string[] sourceValueEntries;
                WildcardPattern[] fromValuePatterns;
                if (!multipleValues)
                    if (sourceValueString.Contains(','))
                        throw new PSInvalidCastException("InvalidCastEnumCommaAndNoFlags", null,
                            ExtendedTypeSystem.InvalidCastExceptionEnumerationNoFlagAndComma,
                            sourceValue, destinationType);
                    sourceValueEntries = new string[] { sourceValueString };
                    fromValuePatterns = new WildcardPattern[1];
                    if (WildcardPattern.ContainsWildcardCharacters(sourceValueString))
                        fromValuePatterns[0] = WildcardPattern.Get(sourceValueString, ignoreCase ? WildcardOptions.IgnoreCase : WildcardOptions.None);
                        fromValuePatterns[0] = null;
                    sourceValueEntries = sourceValueString.Split(',');
                    fromValuePatterns = new WildcardPattern[sourceValueEntries.Length];
                    for (int i = 0; i < sourceValueEntries.Length; i++)
                        string sourceValueEntry = sourceValueEntries[i];
                        if (WildcardPattern.ContainsWildcardCharacters(sourceValueEntry))
                            fromValuePatterns[i] = WildcardPattern.Get(sourceValueEntry,
                                ignoreCase ? WildcardOptions.IgnoreCase : WildcardOptions.None);
                            fromValuePatterns[i] = null;
                EnumHashEntry enumHashEntry = EnumSingleTypeConverter.GetEnumHashEntry(destinationType);
                string[] names = enumHashEntry.names;
                Array values = enumHashEntry.values;
                UInt64 returnUInt64 = 0;
                StringComparison ignoreCaseOpt;
                if (ignoreCase)
                    ignoreCaseOpt = StringComparison.OrdinalIgnoreCase;
                    ignoreCaseOpt = StringComparison.Ordinal;
                    WildcardPattern fromValuePattern = fromValuePatterns[i];
                    bool foundOne = false;
                    for (int j = 0; j < names.Length; j++)
                        string name = names[j];
                        if (fromValuePattern != null)
                            if (!fromValuePattern.IsMatch(name))
                            if (!string.Equals(sourceValueEntry, name, ignoreCaseOpt))
                        if (!multipleValues && foundOne)
                            object firstValue = Enum.ToObject(destinationType, returnUInt64);
                            object secondValue = Enum.ToObject(destinationType, Convert.ToUInt64(values.GetValue(i), CultureInfo.CurrentCulture));
                            throw new PSInvalidCastException("InvalidCastEnumTwoStringsFoundAndNoFlags", null,
                                ExtendedTypeSystem.InvalidCastExceptionEnumerationMoreThanOneValue,
                                sourceValue, destinationType, firstValue, secondValue);
                        foundOne = true;
                        returnUInt64 |= Convert.ToUInt64(values.GetValue(j), CultureInfo.CurrentCulture);
                    if (!foundOne)
                        throw new PSInvalidCastException("InvalidCastEnumStringNotFound", null,
                            sourceValueEntry, destinationType, EnumSingleTypeConverter.EnumValues(destinationType));
                return Enum.ToObject(destinationType, returnUInt64);
        /// There might be many cast operators in a Type A that take Type A. Each operator will have a
        /// different return type. Because of that we cannot call GetMethod since it would cause a
        /// AmbiguousMatchException. This auxiliary method calls GetMember to find the right method.
        /// <param name="methodName">Either op_Explicit or op_Implicit, at the moment.</param>
        /// <param name="targetType">The type to look for an operator.</param>
        /// <param name="originalType">Type of the only parameter the operator method should have.</param>
        /// <param name="resultType">Return type of the operator method.</param>
        /// <returns>A cast operator method, or null if not found.</returns>
        private static MethodInfo FindCastOperator(string methodName, Type targetType, Type originalType, Type resultType)
            using (typeConversion.TraceScope("Looking for \"{0}\" cast operator.", methodName))
                // Get multiple matched Public & Static methods
                const BindingFlags flagsToUse = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
                var methods = targetType.GetMember(methodName, flagsToUse);
                    if (!resultType.IsAssignableFrom(method.ReturnType))
                    System.Reflection.ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length != 1 || !parameters[0].ParameterType.IsAssignableFrom(originalType))
                    typeConversion.WriteLine("Found \"{0}\" cast operator in type {1}.", methodName, targetType.FullName);
                typeConversion.TraceScope("Cast operator for \"{0}\" not found.", methodName);
        private static object ConvertNumericThroughDouble(object valueToConvert, Type resultType)
            using (typeConversion.TraceScope("Numeric Conversion through System.Double."))
                // Eventual exceptions here are caught by the caller
                object intermediate = Convert.ChangeType(valueToConvert, typeof(double),
                                                      System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                return Convert.ChangeType(intermediate, resultType,
        private static ManagementObject ConvertToWMI(object valueToConvert,
                                                     Type resultType,
                                                     bool recursion,
                                                     PSObject originalValueToConvert,
                                                     IFormatProvider formatProvider,
                                                     TypeTable backupTable)
            typeConversion.WriteLine("Standard type conversion to a ManagementObject.");
            string valueToConvertString;
                valueToConvertString = PSObject.ToString(null, valueToConvert, "\n", null, null, true, true);
            catch (ExtendedTypeSystemException e)
                typeConversion.WriteLine("Exception converting value to string: {0}", e.Message);
                throw new PSInvalidCastException("InvalidCastGetStringToWMI", e,
                    ExtendedTypeSystem.InvalidCastExceptionNoStringForConversion,
                    resultType.ToString(), e.Message);
                ManagementObject wmiObject = new ManagementObject(valueToConvertString);
                // ManagementObject will not throw if path does not contain valid key
                if (wmiObject.SystemProperties["__CLASS"] == null)
                    string message = StringUtil.Format(ExtendedTypeSystem.InvalidWMIPath, valueToConvertString);
                    throw new PSInvalidCastException(message);
                return wmiObject;
            catch (Exception wmiObjectException)
                typeConversion.WriteLine("Exception creating WMI object: \"{0}\".", wmiObjectException.Message);
                throw new PSInvalidCastException("InvalidCastToWMI", wmiObjectException,
                    ExtendedTypeSystem.InvalidCastExceptionWithInnerException,
                    valueToConvert.ToString(), resultType.ToString(), wmiObjectException.Message);
        private static ManagementObjectSearcher ConvertToWMISearcher(object valueToConvert,
            typeConversion.WriteLine("Standard type conversion to a collection of ManagementObjects.");
                throw new PSInvalidCastException("InvalidCastGetStringToWMISearcher", e,
                ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher(valueToConvertString);
                return objectSearcher;
            catch (Exception objectSearcherException)
                typeConversion.WriteLine("Exception running WMI object query: \"{0}\".", objectSearcherException.Message);
                throw new PSInvalidCastException("InvalidCastToWMISearcher", objectSearcherException,
                    valueToConvert.ToString(), resultType.ToString(), objectSearcherException.Message);
        private static ManagementClass ConvertToWMIClass(object valueToConvert,
            typeConversion.WriteLine("Standard type conversion to a ManagementClass.");
                throw new PSInvalidCastException("InvalidCastGetStringToWMIClass", e,
                ManagementClass wmiClass = new System.Management.ManagementClass(valueToConvertString);
                // ManagementClass will not throw if the path specified is not
                // a valid class.
                if (wmiClass.SystemProperties["__CLASS"] == null)
                    string message = StringUtil.Format(ExtendedTypeSystem.InvalidWMIClassPath, valueToConvertString);
                return wmiClass;
            catch (Exception wmiClassException)
                typeConversion.WriteLine("Exception creating WMI class: \"{0}\".", wmiClassException.Message);
                throw new PSInvalidCastException("InvalidCastToWMIClass", wmiClassException,
                    valueToConvert.ToString(), resultType.ToString(), wmiClassException.Message);
        private static DirectoryEntry ConvertToADSI(object valueToConvert,
            typeConversion.WriteLine("Standard type conversion to DirectoryEntry.");
                throw new PSInvalidCastException("InvalidCastGetStringToADSIClass", e,
                DirectoryEntry entry = new DirectoryEntry(valueToConvertString);
            catch (Exception adsiClassException)
                typeConversion.WriteLine("Exception creating ADSI class: \"{0}\".", adsiClassException.Message);
                throw new PSInvalidCastException("InvalidCastToADSIClass", adsiClassException,
                    valueToConvert.ToString(), resultType.ToString(), adsiClassException.Message);
        private static DirectorySearcher ConvertToADSISearcher(object valueToConvert,
            typeConversion.WriteLine("Standard type conversion to ADSISearcher");
                return new DirectorySearcher((string)valueToConvert);
                typeConversion.WriteLine("Exception creating ADSI searcher: \"{0}\".", e.Message);
                throw new PSInvalidCastException("InvalidCastToADSISearcher", e,
                    valueToConvert.ToString(), resultType.ToString(), e.Message);
        private static StringCollection ConvertToStringCollection(object valueToConvert,
            typeConversion.WriteLine("Standard type conversion to a StringCollection.");
            var stringCollection = new StringCollection();
            AddItemsToCollection(valueToConvert, resultType, formatProvider, backupTable, stringCollection);
            return stringCollection;
        private static void AddItemsToCollection(object valueToConvert, Type resultType, IFormatProvider formatProvider, TypeTable backupTable, StringCollection stringCollection)
                var stringArrayValue = (string[])ConvertTo(valueToConvert, typeof(string[]), false, formatProvider, backupTable);
                stringCollection.AddRange(stringArrayValue);
            catch (PSInvalidCastException)
                typeConversion.WriteLine("valueToConvert contains non-string type values");
                var argEx = new ArgumentException(StringUtil.Format(ExtendedTypeSystem.CannotConvertValueToStringArray, valueToConvert.ToString()));
                throw new PSInvalidCastException(StringUtil.Format("InvalidCastTo{0}Class", resultType.Name), argEx,
                    ExtendedTypeSystem.InvalidCastExceptionWithInnerException, valueToConvert.ToString(), resultType.ToString(), argEx.Message);
                typeConversion.WriteLine("Exception creating StringCollection class: \"{0}\".", ex.Message);
                throw new PSInvalidCastException(StringUtil.Format("InvalidCastTo{0}Class", resultType.Name), ex,
                    ExtendedTypeSystem.InvalidCastExceptionWithInnerException, valueToConvert.ToString(), resultType.ToString(), ex.Message);
        private static XmlDocument ConvertToXml(object valueToConvert,
            using (typeConversion.TraceScope("Standard type conversion to XmlDocument."))
                    throw new PSInvalidCastException("InvalidCastGetStringToXmlDocument", e,
                    using (TextReader textReader = new StringReader(valueToConvertString))
                        // Win8: 481571 Enforcing "PreserveWhitespace" breaks XML pretty printing
                        XmlReaderSettings settings = InternalDeserializer.XmlReaderSettingsForUntrustedXmlDocument.Clone();
                        settings.IgnoreWhitespace = true;
                        settings.IgnoreProcessingInstructions = false;
                        settings.IgnoreComments = false;
                        XmlReader xmlReader = XmlReader.Create(textReader, settings);
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.PreserveWhitespace = false;
                        xmlDocument.Load(xmlReader);
                        return xmlDocument;
                catch (Exception loadXmlException)
                    typeConversion.WriteLine("Exception loading XML: \"{0}\".", loadXmlException.Message);
                    throw new PSInvalidCastException("InvalidCastToXmlDocument", loadXmlException,
                        valueToConvert.ToString(), resultType.ToString(), loadXmlException.Message);
        private static CultureInfo GetCultureFromFormatProvider(IFormatProvider formatProvider)
            CultureInfo returnValue = formatProvider as CultureInfo ?? CultureInfo.InvariantCulture;
        private static bool IsCustomTypeConversion(object valueToConvert,
                                                   out object result,
                                                   TypeTable backupTypeTable)
            using (typeConversion.TraceScope("Custom type conversion."))
                object baseValueToConvert = PSObject.Base(valueToConvert);
                Type originalType = baseValueToConvert.GetType();
                // first using ConvertTo for the original type
                object valueConverter = GetConverter(originalType, backupTypeTable);
                if ((valueConverter != null))
                    TypeConverter valueTypeConverter = valueConverter as TypeConverter;
                    if (valueTypeConverter != null)
                        typeConversion.WriteLine("Original type's converter is TypeConverter.");
                        if (valueTypeConverter.CanConvertTo(resultType))
                            typeConversion.WriteLine("TypeConverter can convert to resultType.");
                                result = valueTypeConverter.ConvertTo(null, GetCultureFromFormatProvider(formatProvider), baseValueToConvert, resultType);
                                typeConversion.WriteLine("Exception converting with Original type's TypeConverter: \"{0}\".", e.Message);
                                throw new PSInvalidCastException("InvalidCastTypeConvertersConvertTo", e,
                            typeConversion.WriteLine("TypeConverter cannot convert to resultType.");
                    PSTypeConverter valuePSTypeConverter = valueConverter as PSTypeConverter;
                    if (valuePSTypeConverter != null)
                        typeConversion.WriteLine("Original type's converter is PSTypeConverter.");
                        PSObject psValueToConvert = PSObject.AsPSObject(valueToConvert);
                        if (valuePSTypeConverter.CanConvertTo(psValueToConvert, resultType))
                            typeConversion.WriteLine("Original type's PSTypeConverter can convert to resultType.");
                                result = valuePSTypeConverter.ConvertTo(psValueToConvert, resultType, formatProvider, true);
                                typeConversion.WriteLine("Exception converting with Original type's PSTypeConverter: \"{0}\".", e.Message);
                                throw new PSInvalidCastException("InvalidCastPSTypeConvertersConvertTo", e,
                            typeConversion.WriteLine("Original type's PSTypeConverter cannot convert to resultType.");
                s_tracer.WriteLine("No converter found in original type.");
                // now ConvertFrom for the destination type
                valueConverter = GetConverter(resultType, backupTypeTable);
                if (valueConverter != null)
                        typeConversion.WriteLine("Destination type's converter is TypeConverter that can convert from originalType.");
                        if (valueTypeConverter.CanConvertFrom(originalType))
                            typeConversion.WriteLine("Destination type's converter can convert from originalType.");
                                result = valueTypeConverter.ConvertFrom(null, GetCultureFromFormatProvider(formatProvider), baseValueToConvert);
                                typeConversion.WriteLine("Exception converting with Destination type's TypeConverter: \"{0}\".", e.Message);
                                throw new PSInvalidCastException("InvalidCastTypeConvertersConvertFrom", e,
                            typeConversion.WriteLine("Destination type's converter cannot convert from originalType.");
                        typeConversion.WriteLine("Destination type's converter is PSTypeConverter.");
                        if (valuePSTypeConverter.CanConvertFrom(psValueToConvert, resultType))
                                result = valuePSTypeConverter.ConvertFrom(psValueToConvert, resultType, formatProvider, true);
                                typeConversion.WriteLine("Exception converting with Destination type's PSTypeConverter: \"{0}\".", e.Message);
                                throw new PSInvalidCastException("InvalidCastPSTypeConvertersConvertFrom", e,
        private static object ConvertNumericChar(object valueToConvert,
                // Convert char through int to float/double.
                object result = Convert.ChangeType(
                    Convert.ChangeType(valueToConvert, typeof(int), formatProvider), resultType, formatProvider);
                typeConversion.WriteLine("Numeric conversion succeeded.");
                typeConversion.WriteLine("Exception converting with IConvertible: \"{0}\".", e.Message);
                throw new PSInvalidCastException("InvalidCastIConvertible", e,
        private static object ConvertNumeric(object valueToConvert,
                object result = Convert.ChangeType(valueToConvert, resultType, formatProvider);
        private static char[] ConvertStringToCharArray(object valueToConvert,
            Diagnostics.Assert(valueToConvert is string, "Value to convert must be string");
            typeConversion.WriteLine("Returning value to convert's ToCharArray().");
            // This conversion is not wrapped in a try/catch because it can't raise an exception
            // unless the string object has been corrupted.
            return ((string)valueToConvert).ToCharArray();
        private static Regex ConvertStringToRegex(object valueToConvert,
            typeConversion.WriteLine("Returning new RegEx(value to convert).");
                return new Regex((string)valueToConvert);
            catch (Exception regexException)
                typeConversion.WriteLine("Exception in RegEx constructor: \"{0}\".", regexException.Message);
                throw new PSInvalidCastException("InvalidCastFromStringToRegex", regexException,
                    valueToConvert.ToString(), resultType.ToString(), regexException.Message);
        private static Microsoft.Management.Infrastructure.CimSession ConvertStringToCimSession(object valueToConvert,
            typeConversion.WriteLine("Returning CimSession.Create(value to convert).");
                return Microsoft.Management.Infrastructure.CimSession.Create((string)valueToConvert);
            catch (Microsoft.Management.Infrastructure.CimException cimException)
                typeConversion.WriteLine("Exception in CimSession.Create: \"{0}\".", cimException.Message);
                throw new PSInvalidCastException("InvalidCastFromStringToCimSession", cimException,
                    valueToConvert.ToString(), resultType.ToString(), cimException.Message);
        private static Type ConvertStringToType(object valueToConvert,
            Diagnostics.Assert(valueToConvert is string, "Value to convert must be a string");
            Type namedType = TypeResolver.ResolveType((string)valueToConvert, out exception);
            if (namedType == null)
                if (exception is PSInvalidCastException)
                throw new PSInvalidCastException("InvalidCastFromStringToType", exception,
                    valueToConvert.ToString(), ObjectToTypeNameString(valueToConvert), resultType.ToString());
            return namedType;
        /// We need to add this built-in converter because in FullCLR, System.Uri has a TypeConverter attribute
        /// declared: [TypeConverter(typeof(UriTypeConverter))], so the conversion from 'string' to 'Uri' is
        /// actually taken care of by 'UriTypeConverter'. However, the type 'UriTypeConverter' is not available
        /// in CoreCLR, and thus the conversion from 'string' to 'Uri' would show a different behavior.
        /// Therefore, we just add this built-in string-to-uri converter using the same logic 'UriTypeConverter'
        /// is using in FullCLR, so the conversion behavior will be the same on desktop powershell and powershell core.
        private static Uri ConvertStringToUri(object valueToConvert,
                return new Uri((string)valueToConvert, UriKind.RelativeOrAbsolute);
            catch (Exception uriException)
                typeConversion.WriteLine("Exception in Uri constructor: \"{0}\".", uriException.Message);
                throw new PSInvalidCastException("InvalidCastFromStringToUri", uriException,
                    valueToConvert.ToString(), resultType.ToString(), uriException.Message);
        /// Attempts to use Parser.ScanNumber to get the value of a numeric string.
        /// <param name="strToConvert">The string to convert to a number.</param>
        /// <param name="resultType">The resulting value type to convert to.</param>
        /// <param name="result">The resulting numeric value.</param>
        /// True if the parse succeeds, false if a parse exception arises.
        /// In all other cases, an exception will be thrown.
        private static bool TryScanNumber(string strToConvert, Type resultType, out object result)
                var parsedNumber = Parser.ScanNumber(strToConvert, resultType, shouldTryCoercion: false);
                if (resultType == typeof(BigInteger) || parsedNumber is BigInteger)
                    // Convert.ChangeType() cannot be used here as BigInteger is not IConvertible.
                    result = ConvertTo(parsedNumber, resultType);
                result = Convert.ChangeType(
                    parsedNumber,
                // Parse or convert failed
        private static object ConvertStringToInteger(
            object valueToConvert,
            var strToConvert = valueToConvert as string;
            Diagnostics.Assert(strToConvert != null, "Value to convert must be a string");
                IsNumeric(GetTypeCode(resultType)) || resultType == typeof(BigInteger),
                "Result type must be numeric");
            if (strToConvert.Length == 0)
                typeConversion.WriteLine("Returning numeric zero.");
                // BigInteger is not IConvertible and will throw from ChangeType; we know the value we're after is zero.
                if (resultType == typeof(BigInteger))
                    return BigInteger.Zero;
                // This is not wrapped in a try/catch because it can't fail.
                return System.Convert.ChangeType(value: 0, resultType, CultureInfo.InvariantCulture);
            typeConversion.WriteLine("Converting to integer.");
                if (TryScanNumber(strToConvert, resultType, out object result))
                    NumberStyles style = NumberStyles.Integer | NumberStyles.AllowThousands;
                    return BigInteger.Parse(strToConvert, style, NumberFormatInfo.InvariantInfo);
                // Fallback conversion for regular numeric types.
                return GetIntegerSystemConverter(resultType).ConvertFrom(strToConvert);
                // This catch has one extra reason to be generic (Exception e).
                // TypeConverter.ConvertFrom wraps its exceptions in a System.Exception.
                typeConversion.WriteLine("Exception converting to integer: \"{0}\".", e.Message);
                if (e is FormatException)
                    typeConversion.WriteLine("Converting to integer passing through double.");
                        return ConvertNumericThroughDouble(strToConvert, resultType);
                    catch (Exception ex) // swallow non-severe exceptions
                        typeConversion.WriteLine("Exception converting to integer through double: \"{0}\".", ex.Message);
                throw new PSInvalidCastException("InvalidCastFromStringToInteger", e,
                    strToConvert, resultType.ToString(), e.Message);
        private static object ConvertStringToDecimal(object valueToConvert,
                return System.Convert.ChangeType(0, resultType, CultureInfo.InvariantCulture);
            typeConversion.WriteLine("Converting to decimal.");
                typeConversion.WriteLine("Parsing string value to account for multipliers and type suffixes");
                    return Convert.ChangeType(strToConvert, resultType, CultureInfo.InvariantCulture.NumberFormat);
                typeConversion.WriteLine("Exception converting to decimal: \"{0}\". Converting to decimal passing through double.", e.Message);
                throw new PSInvalidCastException("InvalidCastFromStringToDecimal", e,
        private static object ConvertStringToReal(object valueToConvert,
            typeConversion.WriteLine("Converting to double or single.");
                typeConversion.WriteLine("Exception converting to double or single: \"{0}\".", e.Message);
                throw new PSInvalidCastException("InvalidCastFromStringToDoubleOrSingle", e,
        private static object ConvertAssignableFrom(object valueToConvert,
            typeConversion.WriteLine("Result type is assignable from value to convert's type");
            return valueToConvert;
        private static PSObject ConvertToPSObject(object valueToConvert,
            typeConversion.WriteLine("Returning PSObject.AsPSObject(valueToConvert).");
            return PSObject.AsPSObject(valueToConvert);
        private static object ConvertToVoid(object valueToConvert,
            typeConversion.WriteLine("returning AutomationNull.Value.");
        private static bool ConvertClassToBool(object valueToConvert,
            typeConversion.WriteLine("Converting ref to boolean.");
            return valueToConvert != null;
        private static bool ConvertValueToBool(object valueToConvert,
            typeConversion.WriteLine("Converting value to boolean.");
        private static bool ConvertStringToBool(object valueToConvert,
            typeConversion.WriteLine("Converting string to boolean.");
            return LanguagePrimitives.IsTrue((string)valueToConvert);
        private static bool ConvertInt16ToBool(object valueToConvert,
            return ((Int16)valueToConvert) != default(Int16);
        private static bool ConvertInt32ToBool(object valueToConvert,
            return ((Int32)valueToConvert) != default(Int32);
        private static bool ConvertInt64ToBool(object valueToConvert,
            return ((Int64)valueToConvert) != default(Int64);
        private static bool ConvertUInt16ToBool(object valueToConvert,
            return ((UInt16)valueToConvert) != default(UInt16);
        private static bool ConvertUInt32ToBool(object valueToConvert,
            return ((UInt32)valueToConvert) != default(UInt32);
        private static bool ConvertUInt64ToBool(object valueToConvert,
            return ((UInt64)valueToConvert) != default(UInt64);
        private static bool ConvertSByteToBool(object valueToConvert,
            return ((sbyte)valueToConvert) != default(sbyte);
        private static bool ConvertByteToBool(object valueToConvert,
            return ((byte)valueToConvert) != default(byte);
        private static bool ConvertSingleToBool(object valueToConvert,
            return ((Single)valueToConvert) != default(Single);
        private static bool ConvertDoubleToBool(object valueToConvert,
            return ((double)valueToConvert) != default(double);
        private static bool ConvertDecimalToBool(object valueToConvert,
            return ((Decimal)valueToConvert) != default(Decimal);
        private static bool ConvertBigIntegerToBool(
                => ((BigInteger)valueToConvert) != BigInteger.Zero;
        private static object ConvertBoolToBigInteger(
                => (bool)valueToConvert ? BigInteger.One : BigInteger.Zero;
        private static PSConverter<bool> CreateNumericToBoolConverter(Type fromType)
            Diagnostics.Assert(LanguagePrimitives.IsNumeric(fromType.GetTypeCode()), "Can only convert numeric types");
            var valueToConvert = Expression.Parameter(typeof(object));
            var parameters = new ParameterExpression[]
                valueToConvert,
                Expression.Parameter(typeof(Type)),
                Expression.Parameter(typeof(bool)),
                Expression.Parameter(typeof(PSObject)),
                Expression.Parameter(typeof(IFormatProvider)),
                Expression.Parameter(typeof(TypeTable))
            return Expression.Lambda<PSConverter<bool>>(
                Expression.NotEqual(Expression.Default(fromType), valueToConvert.Cast(fromType)),
                parameters).Compile();
        private static bool ConvertCharToBool(object valueToConvert,
            typeConversion.WriteLine("Converting char to boolean.");
            char c = (char)valueToConvert;
            return c != '\0';
        private static bool ConvertSwitchParameterToBool(object valueToConvert,
            typeConversion.WriteLine("Converting SwitchParameter to boolean.");
            return ((SwitchParameter)valueToConvert).ToBool();
        private static bool ConvertIListToBool(object valueToConvert,
            typeConversion.WriteLine("Converting IList to boolean.");
            return IsTrue((IList)valueToConvert);
        private static string ConvertNumericToString(object valueToConvert,
            if (originalValueToConvert != null && originalValueToConvert.TokenText != null)
                return originalValueToConvert.TokenText;
            typeConversion.WriteLine("Converting numeric to string.");
                // Ignore formatProvider here, the conversion should be culture invariant.
                var numberFormat = CultureInfo.InvariantCulture.NumberFormat;
                if (valueToConvert is double dbl)
                    return dbl.ToString(DoublePrecision, numberFormat);
                if (valueToConvert is float sgl)
                    return sgl.ToString(SinglePrecision, numberFormat);
                if (valueToConvert is BigInteger b)
                    return b.ToString(numberFormat);
                return (string)Convert.ChangeType(valueToConvert, resultType, CultureInfo.InvariantCulture.NumberFormat);
                typeConversion.WriteLine("Converting numeric to string Exception: \"{0}\".", e.Message);
                throw new PSInvalidCastException("InvalidCastFromNumericToString", e,
        private static string ConvertNonNumericToString(object valueToConvert,
                typeConversion.WriteLine("Converting object to string.");
                return PSObject.ToStringParser(ecFromTLS, valueToConvert, formatProvider);
                typeConversion.WriteLine("Converting object to string Exception: \"{0}\".", e.Message);
                throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", e,
        private static Hashtable ConvertIDictionaryToHashtable(object valueToConvert,
            typeConversion.WriteLine("Converting to Hashtable.");
            return new Hashtable(valueToConvert as IDictionary);
        private static PSReference ConvertToPSReference(object valueToConvert,
            typeConversion.WriteLine("Converting to PSReference.");
            Dbg.Assert(valueToConvert != null, "[ref]$null cast should be handler earlier with a separate ConvertNullToPSReference method");
            return PSReference.CreateInstance(valueToConvert, valueToConvert.GetType());
        private static Delegate ConvertScriptBlockToDelegate(object valueToConvert,
                return ((ScriptBlock)valueToConvert).GetDelegate(resultType);
            typeConversion.WriteLine("Converting script block to delegate Exception: \"{0}\".", exception.Message);
            throw new PSInvalidCastException("InvalidCastFromScriptBlockToDelegate", exception,
                valueToConvert.ToString(), resultType.ToString(), exception.Message);
        private static object ConvertToNullable(object valueToConvert,
            // The CLR doesn't support boxed Nullable<T>.  Instead, languages convert to T and box.
            return ConvertTo(valueToConvert, Nullable.GetUnderlyingType(resultType), recursion, formatProvider, backupTable);
        private static object ConvertRelatedArrays(object valueToConvert,
            typeConversion.WriteLine("The element type of result is assignable from the element type of the value to convert");
            var originalAsArray = (Array)valueToConvert;
            var newValue = Array.CreateInstance(resultType.GetElementType(), originalAsArray.Length);
            originalAsArray.CopyTo(newValue, 0);
            return newValue;
        private static object ConvertUnrelatedArrays(object valueToConvert,
            Array valueAsArray = valueToConvert as Array;
            Type resultElementType = resultType.GetElementType();
            Array resultArray = Array.CreateInstance(resultElementType, valueAsArray.Length);
            for (int i = 0; i < valueAsArray.Length; i++)
                object resultElement = ConvertTo(valueAsArray.GetValue(i), resultElementType, false, formatProvider, backupTable);
                resultArray.SetValue(resultElement, i);
            return resultArray;
        private static object ConvertEnumerableToArray(object valueToConvert,
                Type resultElementType = resultType == typeof(Array) ? typeof(object) : resultType.GetElementType();
                typeConversion.WriteLine("Converting elements in the value to convert to the result's element type.");
                foreach (object obj in LanguagePrimitives.GetEnumerable(valueToConvert))
                    // false means no further recursions and therefore no cycles
                    result.Add(ConvertTo(obj, resultElementType, false, formatProvider, backupTable));
                return result.ToArray(resultElementType);
                typeConversion.WriteLine("Element conversion exception: \"{0}\".", e.Message);
                throw new PSInvalidCastException("InvalidCastExceptionEnumerableToArray", e,
        private static object ConvertScalarToArray(object valueToConvert,
            typeConversion.WriteLine("Value to convert is scalar.");
                valueToConvert = originalValueToConvert;
                result.Add(ConvertTo(valueToConvert, resultElementType, false, formatProvider, backupTable));
                throw new PSInvalidCastException("InvalidCastExceptionScalarToArray", e,
        private static object ConvertIntegerToEnum(object valueToConvert,
                result = System.Enum.ToObject(resultType, valueToConvert);
                typeConversion.WriteLine("Integer to System.Enum exception: \"{0}\".", e.Message);
                throw new PSInvalidCastException("InvalidCastExceptionIntegerToEnum", e,
            // Check if the result is a defined enum..otherwise throw an error
            // valueToConvert is the user supplied value. Use that in the error message.
            EnumSingleTypeConverter.ThrowForUndefinedEnum("UndefinedIntegerToEnum", result, valueToConvert, resultType);
        private static object ConvertStringToEnum(object valueToConvert,
            string valueAsString = valueToConvert as string;
            typeConversion.WriteLine("Calling case sensitive Enum.Parse");
                result = Enum.Parse(resultType, valueAsString);
                typeConversion.WriteLine("Enum.Parse Exception: \"{0}\".", e.Message);
                // Enum.Parse will always throw this kind of exception.
                // Even when no map exists. We want to try without case sensitivity
                // If it works, we will return it, otherwise a new exception will
                // be thrown and we will use it to set exceptionToWrap
                    typeConversion.WriteLine("Calling case insensitive Enum.Parse");
                    result = Enum.Parse(resultType, valueAsString, true);
                catch (ArgumentException inner)
                    typeConversion.WriteLine("Enum.Parse Exception: \"{0}\".", inner.Message);
                catch (Exception ex) // Enum.Parse might throw unadvertised exceptions
                    typeConversion.WriteLine("Case insensitive Enum.Parse threw an exception.");
                    throw new PSInvalidCastException("CaseInsensitiveEnumParseThrewAnException", ex,
                            valueToConvert.ToString(), resultType.ToString(), ex.Message);
            catch (Exception e) // Enum.Parse might throw unadvertised exceptions
                typeConversion.WriteLine("Case Sensitive Enum.Parse threw an exception.");
                throw new PSInvalidCastException("CaseSensitiveEnumParseThrewAnException", e,
                typeConversion.WriteLine("Calling substring disambiguation.");
                    string enumValue = EnumMinimumDisambiguation.EnumDisambiguate(valueAsString, resultType);
                    result = Enum.Parse(resultType, enumValue);
                catch (Exception e) // Wrap exceptions in type conversion exceptions
                    typeConversion.WriteLine("Substring disambiguation threw an exception.");
                    throw new PSInvalidCastException("SubstringDisambiguationEnumParseThrewAnException", e,
            EnumSingleTypeConverter.ThrowForUndefinedEnum("EnumParseUndefined", result, valueToConvert, resultType);
            s_tracer.WriteLine("returning \"{0}\" from conversion to Enum.", result);
        private static object ConvertEnumerableToEnum(object valueToConvert,
            IEnumerator e = LanguagePrimitives.GetEnumerator(valueToConvert);
            StringBuilder sbResult = new StringBuilder();
            bool notFirst = false;
            while (ParserOps.MoveNext(null, null, e))
                if (notFirst)
                    sbResult.Append(',');
                    notFirst = true;
                string current = e.Current as string;
                    // If the object wasn't a string, then we'll try and convert it into an enum value,
                    // then convert the enum back to a string and finally append it to the string builder to
                    // preserve consistent semantics between quoted and unquoted lists...
                    object tempResult = ConvertTo(e.Current, resultType, recursion, formatProvider, backupTable);
                    if (tempResult != null)
                        sbResult.Append(tempResult.ToString());
                            e.Current, resultType, EnumSingleTypeConverter.EnumValues(resultType));
                sbResult.Append(current);
            return ConvertStringToEnum(sbResult.ToString(), resultType, recursion, originalValueToConvert, formatProvider, backupTable);
        private sealed class PSMethodToDelegateConverter
            // Index of the matching overload method.
            private readonly int _matchIndex;
            // Size of the cache. It's rare to have more than 10 overloads for a method.
            private const int CacheSize = 10;
            private static readonly PSMethodToDelegateConverter[] s_converterCache = new PSMethodToDelegateConverter[CacheSize];
            private PSMethodToDelegateConverter(int matchIndex)
                _matchIndex = matchIndex;
            internal static PSMethodToDelegateConverter GetConverter(int matchIndex)
                if (matchIndex >= CacheSize) { return new PSMethodToDelegateConverter(matchIndex); }
                var result = s_converterCache[matchIndex];
                    // If the cache entry is null, generate a new instance for the cache slot.
                    var converter = new PSMethodToDelegateConverter(matchIndex);
                    Threading.Interlocked.CompareExchange(ref s_converterCache[matchIndex], converter, null);
                    result = s_converterCache[matchIndex];
            internal Delegate Convert(object valueToConvert,
                // We can only possibly convert PSMethod instance of the type PSMethod<T>.
                // Such a PSMethod essentially represents a set of .NET method overloads.
                var psMethod = (PSMethod)valueToConvert;
                    var methods = (MethodCacheEntry)psMethod.adapterData;
                    var isStatic = psMethod.instance is Type;
                    var candidate = (MethodInfo)methods.methodInformationStructures[_matchIndex].method;
                    return isStatic ? candidate.CreateDelegate(resultType)
                                    : candidate.CreateDelegate(resultType, psMethod.instance);
                    typeConversion.WriteLine("PSMethod to Delegate exception: \"{0}\".", e.Message);
                    throw new PSInvalidCastException("InvalidCastExceptionPSMethodToDelegate", e,
        private sealed class ConvertViaParseMethod
            // TODO - use an ETS wrapper that generates a dynamic method
            internal MethodInfo parse;
            internal object ConvertWithCulture(object valueToConvert,
                    object result = parse.Invoke(null, new object[2] { valueToConvert, formatProvider });
                    typeConversion.WriteLine("Parse result: {0}", result);
                    typeConversion.WriteLine("Exception calling Parse method with CultureInfo: \"{0}\".", inner.Message);
                    throw new PSInvalidCastException("InvalidCastParseTargetInvocationWithFormatProvider", inner,
                        valueToConvert.ToString(), resultType.ToString(), inner.Message);
                    typeConversion.WriteLine("Exception calling Parse method with CultureInfo: \"{0}\".", e.Message);
                    throw new PSInvalidCastException("InvalidCastParseExceptionWithFormatProvider", e,
            internal object ConvertWithoutCulture(object valueToConvert,
                    object result = parse.Invoke(null, new object[1] { valueToConvert });
                    typeConversion.WriteLine("Parse result: \"{0}\".", result);
                    typeConversion.WriteLine("Exception calling Parse method: \"{0}\".", inner.Message);
                    throw new PSInvalidCastException("InvalidCastParseTargetInvocation", inner,
                    typeConversion.WriteLine("Exception calling Parse method: \"{0}\".", e.Message);
                    throw new PSInvalidCastException("InvalidCastParseException", e,
        private sealed class ConvertViaConstructor
            internal Func<object, object> TargetCtorLambda;
            internal object Convert(object valueToConvert,
                    object result = TargetCtorLambda(valueToConvert);
                    typeConversion.WriteLine("Constructor result: \"{0}\".", result);
                    typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", inner.Message);
                    throw new PSInvalidCastException("InvalidCastConstructorTargetInvocationException", inner,
                    typeConversion.WriteLine("Exception invoking Constructor: \"{0}\".", e.Message);
                    throw new PSInvalidCastException("InvalidCastConstructorException", e,
        /// Create a IList to hold all elements, and use the IList to create the object of the resultType.
        /// The reason for using IList is that it can work on constructors that takes IEnumerable[T], ICollection[T] or IList[T].
        /// When get to this method, we know the fromType and the toType meet the following two conditions:
        /// 1. toType is a closed generic type and it has a constructor that takes IEnumerable[T], ICollection[T] or IList[T]
        /// 2. fromType is System.Array, System.Object[] or it's the same as the element type of toType
        private sealed class ConvertViaIEnumerableConstructor
            internal Func<int, IList> ListCtorLambda;
            internal Func<IList, object> TargetCtorLambda;
            internal Type ElementType;
            internal bool IsScalar;
                IList resultAsList = null;
                Array array = null;
                    int listSize = 0;
                    if (IsScalar)
                        listSize = 1;
                        // typeof(Array).IsAssignableFrom(typeof(object[])) == true
                        array = valueToConvert as Array;
                        listSize = array != null ? array.Length : 1;
                    resultAsList = ListCtorLambda(listSize);
                    ThrowInvalidCastException(valueToConvert, resultType);
                    Diagnostics.Assert(false, "ThrowInvalidCastException always throws");
                    resultAsList.Add(valueToConvert);
                else if (array == null)
                    object convertedValue = LanguagePrimitives.ConvertTo(valueToConvert, ElementType, formatProvider);
                    resultAsList.Add(convertedValue);
                    foreach (object item in array)
                        object baseObj = PSObject.Base(item);
                        object convertedValue;
                        if (!LanguagePrimitives.TryConvertTo(baseObj, ElementType, formatProvider, out convertedValue))
                    object result = TargetCtorLambda(resultAsList);
                    typeConversion.WriteLine("IEnumerable Constructor result: \"{0}\".", result);
                    typeConversion.WriteLine("Exception invoking IEnumerable Constructor: \"{0}\".", inner.Message);
                    typeConversion.WriteLine("Exception invoking IEnumerable Constructor: \"{0}\".", e.Message);
        private sealed class ConvertViaNoArgumentConstructor
            private readonly Func<object> _constructor;
            internal ConvertViaNoArgumentConstructor(ConstructorInfo constructor, Type type)
                var newExpr = (constructor != null) ? Expression.New(constructor) : Expression.New(type);
                _constructor = Expression.Lambda<Func<object>>(newExpr.Cast(typeof(object))).Compile();
                return Convert(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTable, false);
                        TypeTable backupTable, bool ignoreUnknownMembers)
                    // Setting arbitrary properties is dangerous, so we allow this only if
                    //  - It's running on a thread without Runspace; Or
                    //  - It's in FullLanguage but not because it's part of a parameter binding that is transitioning from ConstrainedLanguage to FullLanguage
                    // When this is invoked from a parameter binding in transition from ConstrainedLanguage environment to FullLanguage command, we disallow
                    // the property conversion because it's dangerous.
                    bool canProceedWithConversion = ecFromTLS == null || (ecFromTLS.LanguageMode == PSLanguageMode.FullLanguage && !ecFromTLS.LanguageModeTransitionInParameterBinding);
                    if (!canProceedWithConversion)
                            throw InterpreterError.NewInterpreterException(
                                typeof(RuntimeException),
                                "HashtableToObjectConversionNotSupportedInDataSection",
                                ParserStrings.HashtableToObjectConversionNotSupportedInDataSection,
                                resultType.ToString());
                        // When in audit mode, we report but don't enforce, so we will proceed with the conversion.
                                context: ecFromTLS,
                                title: ExtendedTypeSystem.WDACHashTypeLogTitle,
                                message: StringUtil.Format(ExtendedTypeSystem.WDACHashTypeLogMessage, resultType.FullName),
                                fqid: "LanguageHashtableConversionNotAllowed",
                    result = _constructor();
                    var psobject = valueToConvert as PSObject;
                    if (psobject != null)
                        // Use PSObject properties to perform conversion.
                        SetObjectProperties(result, psobject, resultType, CreateMemberNotFoundError, CreateMemberSetValueError, formatProvider, recursion, ignoreUnknownMembers);
                        // Use provided property dictionary to perform conversion.
                        IDictionary properties = valueToConvert as IDictionary;
                        SetObjectProperties(result, properties, resultType, CreateMemberNotFoundError, CreateMemberSetValueError, enableMethodCall: false);
                    Exception inner = e.InnerException ?? e;
                    throw new PSInvalidCastException("ObjectCreationError", e,
                        ExtendedTypeSystem.ObjectCreationError, resultType.ToString(), inner.Message);
                catch (SetValueException e)
                    throw new PSInvalidCastException("ObjectCreationError", inner,
                    throw new PSInvalidCastException("InvalidCastConstructorException", inner,
        private sealed class ConvertViaCast
            internal MethodInfo cast;
                    return cast.Invoke(null, new object[1] { valueToConvert });
                    typeConversion.WriteLine("Cast operator exception: \"{0}\".", inner.Message);
                    throw new PSInvalidCastException("InvalidCastTargetInvocationException" + cast.Name, inner,
                    typeConversion.WriteLine("Cast operator exception: \"{0}\".", e.Message);
                    throw new PSInvalidCastException("InvalidCastException" + cast.Name, e,
        private static object ConvertIConvertible(object valueToConvert,
                typeConversion.WriteLine("Conversion using IConvertible succeeded.");
        private static object ConvertNumericIConvertible(object valueToConvert,
            // If the original object was a number, then try and do a conversion on the string
            // equivalent of that number...
                return LanguagePrimitives.ConvertTo(originalValueToConvert.TokenText,
                    resultType, recursion, formatProvider, backupTable);
                // Convert the source object to a string...
                string s = (string)LanguagePrimitives.ConvertTo(valueToConvert,
                    typeof(string), recursion, formatProvider, backupTable);
                // And try and convert that string to the target type...
                return LanguagePrimitives.ConvertTo(s,
        private sealed class ConvertCheckingForCustomConverter
            internal PSConverter<object> tryfirstConverter;
            internal PSConverter<object> fallbackConverter;
                if (tryfirstConverter != null)
                        return tryfirstConverter(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTable);
                if (IsCustomTypeConversion(originalValueToConvert ?? valueToConvert, resultType, formatProvider, out result, backupTable))
                    typeConversion.WriteLine("Custom Type Conversion succeeded.");
                if (fallbackConverter != null)
                    return fallbackConverter(valueToConvert, resultType, recursion, originalValueToConvert, formatProvider, backupTable);
                throw new PSInvalidCastException("ConvertToFinalInvalidCastException", null,
        #region Delegates converting null
        private static object ConvertNullToNumeric(
            typeConversion.WriteLine("Converting null to zero.");
            // Handle BigInteger first, as it is not IConvertible
            // If the destination type is numeric, convert 0 to resultType
        private static char ConvertNullToChar(object valueToConvert,
            typeConversion.WriteLine("Converting null to '0'.");
        private static string ConvertNullToString(object valueToConvert,
            typeConversion.WriteLine("Converting null to \"\".");
            // if the destination type is string, return an empty string...
        private static PSReference ConvertNullToPSReference(object valueToConvert,
            return new PSReference<Null>(null);
        private static object ConvertNullToRef(object valueToConvert,
            // if the target type is not a value type, return the original
            // "null" object. Don't just return null because we want to preserve
            // an msh object if possible.
        private static bool ConvertNullToBool(object valueToConvert,
            typeConversion.WriteLine("Converting null to boolean.");
        private static object ConvertNullToNullable(object valueToConvert,
        private static SwitchParameter ConvertNullToSwitch(object valueToConvert,
            typeConversion.WriteLine("Converting null to SwitchParameter(false).");
            return new SwitchParameter(false);
        private static object ConvertNullToVoid(object valueToConvert,
            typeConversion.WriteLine("Converting null to AutomationNull.Value.");
        #endregion Delegates converting null
        private static object ConvertNoConversion(object valueToConvert,
        private static object ConvertNotSupportedConversion(object valueToConvert,
            ThrowInvalidConversionException(valueToConvert, resultType);
        [System.Diagnostics.DebuggerDisplay("{from.Name}->{to.Name}")]
        private struct ConversionTypePair
            internal Type from;
            internal Type to;
            internal ConversionTypePair(Type fromType, Type toType)
                from = fromType;
                to = toType;
                    // To prevent to/from == from/to, multiply and add rather than use
                    // an operation that won't overflow, like bitwise xor.
                    return from.GetHashCode() + 37 * to.GetHashCode();
            public override bool Equals(object other)
                if (other is not ConversionTypePair)
                var ctp = (ConversionTypePair)other;
                return this.from == ctp.from && this.to == ctp.to;
        internal delegate T PSConverter<T>(object valueToConvert,
                                           TypeTable backupTable);
        internal delegate object PSNullConverter(object nullOrAutomationNull);
        internal interface IConversionData
            object Converter { get; }
            ConversionRank Rank { get; }
            object? Invoke(object? valueToConvert,
                          PSObject? originalValueToConvert,
                          IFormatProvider? formatProvider,
                          TypeTable? backupTable);
        [System.Diagnostics.DebuggerDisplay("{_converter.Method.Name}")]
        internal class ConversionData<T> : IConversionData
            private readonly PSConverter<T> _converter;
            public ConversionData(PSConverter<T> converter, ConversionRank rank)
                _converter = converter;
                Rank = rank;
            public object Converter
                get { return _converter; }
            public ConversionRank Rank { get; }
            public object Invoke(object valueToConvert, Type resultType, bool recurse, PSObject originalValueToConvert, IFormatProvider formatProvider, TypeTable backupTable)
                return _converter.Invoke(valueToConvert, resultType, recurse, originalValueToConvert, formatProvider, backupTable);
        private static readonly Dictionary<ConversionTypePair, IConversionData> s_converterCache = new Dictionary<ConversionTypePair, IConversionData>(256);
        private static IConversionData CacheConversion<T>(Type fromType, Type toType, PSConverter<T> converter, ConversionRank rank)
            ConversionTypePair pair = new ConversionTypePair(fromType, toType);
            IConversionData data = null;
                if (!s_converterCache.TryGetValue(pair, out data))
                    data = new ConversionData<T>(converter, rank);
                    s_converterCache.Add(pair, data);
                    Dbg.Assert(((Delegate)(data.Converter)).GetMethodInfo().Equals(converter.GetMethodInfo()),
                        "Existing conversion isn't the same as new conversion");
        private static IConversionData GetConversionData(Type fromType, Type toType)
                IConversionData result = null;
                s_converterCache.TryGetValue(new ConversionTypePair(fromType, toType), out result);
        internal static ConversionRank GetConversionRank(Type fromType, Type toType)
            return FigureConversion(fromType, toType).Rank;
        private static readonly Type[] s_numericTypes = new Type[] {
            typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(UInt16), typeof(UInt32), typeof(UInt64),
            typeof(sbyte), typeof(byte),
            typeof(Single), typeof(double), typeof(decimal),
            typeof(BigInteger)
        private static readonly Type[] s_integerTypes = new Type[] {
            typeof(sbyte), typeof(byte)
        // Do not reorder the elements of these arrays, we depend on them being ordered by increasing size.
        private static readonly Type[] s_signedIntegerTypes = new Type[] { typeof(sbyte), typeof(Int16), typeof(Int32), typeof(Int64) };
        private static readonly Type[] s_unsignedIntegerTypes = new Type[] { typeof(byte), typeof(UInt16), typeof(UInt32), typeof(UInt64) };
        private static readonly Type[] s_realTypes = new Type[] { typeof(Single), typeof(double), typeof(decimal) };
        internal static void RebuildConversionCache()
                s_converterCache.Clear();
                Type typeofString = typeof(string);
                Type typeofNull = typeof(Null);
                Type typeofFloat = typeof(float);
                Type typeofDouble = typeof(double);
                Type typeofDecimal = typeof(decimal);
                Type typeofBool = typeof(bool);
                Type typeofChar = typeof(char);
                foreach (Type type in LanguagePrimitives.s_numericTypes)
                    CacheConversion<string>(type, typeofString, LanguagePrimitives.ConvertNumericToString, ConversionRank.NumericString);
                    CacheConversion<object>(type, typeofChar, LanguagePrimitives.ConvertIConvertible, ConversionRank.NumericString);
                    CacheConversion<object>(typeofNull, type, LanguagePrimitives.ConvertNullToNumeric, ConversionRank.NullToValue);
                CacheConversion<object>(typeofBool, typeof(BigInteger), ConvertBoolToBigInteger, ConversionRank.Language);
                CacheConversion<bool>(typeof(Int16), typeofBool, ConvertInt16ToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(Int32), typeofBool, ConvertInt32ToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(Int64), typeofBool, ConvertInt64ToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(UInt16), typeofBool, ConvertUInt16ToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(UInt32), typeofBool, ConvertUInt32ToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(UInt64), typeofBool, ConvertUInt64ToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(sbyte), typeofBool, ConvertSByteToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(byte), typeofBool, ConvertByteToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(Single), typeofBool, ConvertSingleToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(double), typeofBool, ConvertDoubleToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(decimal), typeofBool, ConvertDecimalToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(BigInteger), typeofBool, ConvertBigIntegerToBool, ConversionRank.Language);
                for (int i = 0; i < LanguagePrimitives.s_unsignedIntegerTypes.Length; i++)
                    // Identical types are an identity conversion.
                    CacheConversion<object>(s_unsignedIntegerTypes[i], s_unsignedIntegerTypes[i],
                                            LanguagePrimitives.ConvertAssignableFrom, ConversionRank.Identity);
                    CacheConversion<object>(s_signedIntegerTypes[i], s_signedIntegerTypes[i],
                    // Unsigned to signed same size is explicit
                    CacheConversion<object>(s_unsignedIntegerTypes[i], s_signedIntegerTypes[i],
                                            LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit);
                    // Signed to unsigned same size is explicit, but better than the reverse (because it is "more specific")
                    CacheConversion<object>(s_signedIntegerTypes[i], s_unsignedIntegerTypes[i],
                                            LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit1);
                    for (int j = i + 1; j < LanguagePrimitives.s_unsignedIntegerTypes.Length; j++)
                        // Conversions where the sign doesn't change, but the size is bigger, is implicit
                        CacheConversion<object>(s_unsignedIntegerTypes[i], s_unsignedIntegerTypes[j],
                                                LanguagePrimitives.ConvertNumeric, ConversionRank.NumericImplicit);
                        CacheConversion<object>(s_signedIntegerTypes[i], s_signedIntegerTypes[j],
                        // Conversion from smaller unsigned to bigger signed is implicit
                        CacheConversion<object>(s_unsignedIntegerTypes[i], s_signedIntegerTypes[j],
                        // Conversion from smaller signed to bigger unsigned is the "better" explicit conversion
                        CacheConversion<object>(s_signedIntegerTypes[i], s_unsignedIntegerTypes[j],
                        // Conversion to a smaller type is explicit
                        CacheConversion<object>(s_unsignedIntegerTypes[j], s_unsignedIntegerTypes[i],
                        CacheConversion<object>(s_signedIntegerTypes[j], s_signedIntegerTypes[i],
                        CacheConversion<object>(s_unsignedIntegerTypes[j], s_signedIntegerTypes[i],
                        CacheConversion<object>(s_signedIntegerTypes[j], s_unsignedIntegerTypes[i],
                foreach (Type integerType in s_integerTypes)
                    CacheConversion<object>(typeofString, integerType, LanguagePrimitives.ConvertStringToInteger, ConversionRank.NumericString);
                    foreach (Type realType in s_realTypes)
                        CacheConversion<object>(integerType, realType, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericImplicit);
                        CacheConversion<object>(realType, integerType, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit);
                CacheConversion<object>(typeofString, typeof(BigInteger), ConvertStringToInteger, ConversionRank.NumericString);
                CacheConversion<object>(typeofFloat, typeofDouble, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericImplicit);
                CacheConversion<object>(typeofDouble, typeofFloat, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit);
                CacheConversion<object>(typeofFloat, typeofDecimal, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit);
                CacheConversion<object>(typeofDouble, typeofDecimal, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit);
                CacheConversion<object>(typeofDecimal, typeofFloat, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit1);
                CacheConversion<object>(typeofDecimal, typeofDouble, LanguagePrimitives.ConvertNumeric, ConversionRank.NumericExplicit1);
                CacheConversion<Regex>(typeofString, typeof(Regex), LanguagePrimitives.ConvertStringToRegex, ConversionRank.Language);
                CacheConversion<char[]>(typeofString, typeof(char[]), LanguagePrimitives.ConvertStringToCharArray, ConversionRank.StringToCharArray);
                CacheConversion<Type>(typeofString, typeof(Type), LanguagePrimitives.ConvertStringToType, ConversionRank.Language);
                CacheConversion<Uri>(typeofString, typeof(Uri), LanguagePrimitives.ConvertStringToUri, ConversionRank.Language);
                CacheConversion<object>(typeofString, typeofDecimal, LanguagePrimitives.ConvertStringToDecimal, ConversionRank.NumericString);
                CacheConversion<object>(typeofString, typeofFloat, LanguagePrimitives.ConvertStringToReal, ConversionRank.NumericString);
                CacheConversion<object>(typeofString, typeofDouble, LanguagePrimitives.ConvertStringToReal, ConversionRank.NumericString);
                CacheConversion<object>(typeofChar, typeofFloat, LanguagePrimitives.ConvertNumericChar, ConversionRank.Language);
                CacheConversion<object>(typeofChar, typeofDouble, LanguagePrimitives.ConvertNumericChar, ConversionRank.Language);
                CacheConversion<bool>(typeofChar, typeofBool, LanguagePrimitives.ConvertCharToBool, ConversionRank.Language);
                // Conversions from null
                CacheConversion<char>(typeofNull, typeofChar, LanguagePrimitives.ConvertNullToChar, ConversionRank.NullToValue);
                CacheConversion<string>(typeofNull, typeofString, LanguagePrimitives.ConvertNullToString, ConversionRank.ToString);
                CacheConversion<bool>(typeofNull, typeofBool, LanguagePrimitives.ConvertNullToBool, ConversionRank.NullToValue);
                CacheConversion<PSReference>(typeofNull, typeof(PSReference), LanguagePrimitives.ConvertNullToPSReference, ConversionRank.NullToRef);
                CacheConversion<SwitchParameter>(typeofNull, typeof(SwitchParameter), LanguagePrimitives.ConvertNullToSwitch, ConversionRank.NullToValue);
                CacheConversion<object>(typeofNull, typeof(void), LanguagePrimitives.ConvertNullToVoid, ConversionRank.NullToValue);
                // Conversions to bool
                CacheConversion<object>(typeofBool, typeofBool, LanguagePrimitives.ConvertAssignableFrom, ConversionRank.Identity);
                CacheConversion<bool>(typeofString, typeofBool, LanguagePrimitives.ConvertStringToBool, ConversionRank.Language);
                CacheConversion<bool>(typeof(SwitchParameter), typeofBool, LanguagePrimitives.ConvertSwitchParameterToBool, ConversionRank.Language);
                // Conversions to WMI and ADSI
                CacheConversion<ManagementObjectSearcher>(typeofString, typeof(ManagementObjectSearcher), LanguagePrimitives.ConvertToWMISearcher, ConversionRank.Language);
                CacheConversion<ManagementClass>(typeofString, typeof(ManagementClass), LanguagePrimitives.ConvertToWMIClass, ConversionRank.Language);
                CacheConversion<ManagementObject>(typeofString, typeof(ManagementObject), LanguagePrimitives.ConvertToWMI, ConversionRank.Language);
                CacheConversion<DirectoryEntry>(typeofString, typeof(DirectoryEntry), LanguagePrimitives.ConvertToADSI, ConversionRank.Language);
                CacheConversion<DirectorySearcher>(typeofString, typeof(DirectorySearcher), LanguagePrimitives.ConvertToADSISearcher, ConversionRank.Language);
        internal static PSObject SetObjectProperties(object o, PSObject psObject, Type resultType, MemberNotFoundError memberNotFoundErrorAction, MemberSetValueError memberSetValueErrorAction, IFormatProvider formatProvider, bool recursion = false, bool ignoreUnknownMembers = false)
            // Type conversion from object properties only supported for deserialized types.
            if (Deserializer.IsDeserializedInstanceOfType(psObject, resultType))
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    foreach (var item in psObject.Properties)
                        if (item is PSProperty)
                            properties.Add(item.Name, item.Value);
                    return SetObjectProperties(o, properties, resultType, memberNotFoundErrorAction, memberSetValueErrorAction, enableMethodCall: false);
                catch (SetValueException)
                object baseObj = PSObject.Base(psObject);
                var dictionary = baseObj as IDictionary;
                    return SetObjectProperties(o, dictionary, resultType, memberNotFoundErrorAction, memberSetValueErrorAction, enableMethodCall: false);
                    // Support PSObject to Strong type conversion.
                    PSObject psBaseObject = baseObj as PSObject;
                    if (psBaseObject != null)
                        foreach (var item in psBaseObject.Properties)
                            return SetObjectProperties(o, properties, resultType, memberNotFoundErrorAction, memberSetValueErrorAction, false, formatProvider, recursion, ignoreUnknownMembers);
                            throw new PSInvalidCastException("ConvertToFinalInvalidCastException", exception,
                                 psObject.ToString(), ObjectToTypeNameString(psObject),
            ThrowInvalidCastException(psObject, resultType);
        internal static PSObject SetObjectProperties(object o, IDictionary properties, Type resultType, MemberNotFoundError memberNotFoundErrorAction, MemberSetValueError memberSetValueErrorAction, bool enableMethodCall)
            return SetObjectProperties(o, properties, resultType, memberNotFoundErrorAction, memberSetValueErrorAction, enableMethodCall, CultureInfo.InvariantCulture, false, false);
        internal static PSObject SetObjectProperties(object o, IDictionary properties, Type resultType, MemberNotFoundError memberNotFoundErrorAction, MemberSetValueError memberSetValueErrorAction, bool enableMethodCall, IFormatProvider formatProvider, bool recursion = false, bool ignoreUnknownMembers = false)
                foreach (DictionaryEntry prop in properties)
                    PSMethodInfo method = enableMethodCall ? pso.Methods[prop.Key.ToString()] : null;
                            method.Invoke(new object[] { prop.Value });
                            PSPropertyInfo property = pso.Properties[prop.Key.ToString()];
                                object propValue = prop.Value;
                                if (recursion && prop.Value != null)
                                    Type propType;
                                    if (TypeResolver.TryResolveType(property.TypeNameOfValue, out propType))
                                            PSObject propertyValue = prop.Value as PSObject;
                                                propValue = LanguagePrimitives.ConvertPSObjectToType(propertyValue, propType, recursion, formatProvider, ignoreUnknownMembers);
                                            else if (prop.Value is PSCustomObject)
                                                propValue = LanguagePrimitives.ConvertPSObjectToType(new PSObject(prop.Value), propType, recursion, formatProvider, ignoreUnknownMembers);
                                                propValue = LanguagePrimitives.ConvertTo(prop.Value, propType, recursion, formatProvider, null);
                                            // We don't care. We will assign the value as is.
                                // treat AutomationNull.Value as null for consistency
                                if (propValue == AutomationNull.Value)
                                    propValue = null;
                                property.Value = propValue;
                                if (pso.BaseObject is PSCustomObject)
                                    var key = prop.Key as string;
                                    var value = prop.Value as string;
                                    if (key != null && value != null && key.Equals("PSTypeName", StringComparison.OrdinalIgnoreCase))
                                        pso.TypeNames.Insert(0, value);
                                        pso.Properties.Add(new PSNoteProperty(prop.Key.ToString(), prop.Value));
                                    if (!ignoreUnknownMembers)
                                        memberNotFoundErrorAction(pso, prop, resultType);
                        memberSetValueErrorAction(e);
            return pso;
        private static string GetSettableProperties(PSObject pso)
            if (pso is null || pso.Properties is null)
            StringBuilder availableProperties = new StringBuilder();
            foreach (PSPropertyInfo p in pso.Properties)
                if (p.IsSettable)
                        availableProperties.Append(", ");
                    availableProperties.Append("[" + p.Name + " <" + p.TypeNameOfValue + ">]");
            return availableProperties.ToString();
        internal static IConversionData FigureConversion(object valueToConvert, Type resultType, out bool debase)
            PSObject valueAsPsObj;
            Type originalType;
            if (valueToConvert == null || valueToConvert == AutomationNull.Value)
                valueAsPsObj = null;
                originalType = typeof(Null);
                valueAsPsObj = valueToConvert as PSObject;
                originalType = valueToConvert.GetType();
            debase = false;
            IConversionData data = FigureConversion(originalType, resultType);
            if (data.Rank != ConversionRank.None)
            if (valueAsPsObj != null)
                debase = true;
                // Now try converting PSObject.Base instead.
                valueToConvert = PSObject.Base(valueToConvert);
                Dbg.Assert(valueToConvert != AutomationNull.Value, "PSObject.Base converts AutomationNull.Value to null");
                if (valueToConvert == null)
                    // If the original value was a property bag (empty PSObject), we won't find a conversion because
                    // all PSObject conversions have already been checked.
                    // Still, there are many valid conversions to allow, such as PSObject to bool, void, or
                    // a custom type converter.  To find those, we consider InternalPSObject=>resultType instead.
                    // We use a different type because we can't keep PSObject as the from type in the cache.
                    originalType = (valueToConvert is PSObject) ? typeof(InternalPSObject) : valueToConvert.GetType();
                data = FigureConversion(originalType, resultType);
        /// <param name="valueToConvert">The same as in the public version.</param>
        /// <param name="resultType">The same as in the public version.</param>
        /// <param name="recursion">True if we should perform any recursive calls to ConvertTo.</param>
        /// <param name="backupTypeTable">
        /// <returns>The value converted.</returns>
        internal static object ConvertTo(object valueToConvert,
                bool debase;
                var conversion = FigureConversion(valueToConvert, resultType, out debase);
                return conversion.Invoke(
                    recursion,
                    backupTypeTable);
        /// Get the errorId and errorMessage for an InvalidCastException.
        /// A two-element tuple indicating [errorId, errorMsg]
        internal static Tuple<string, string> GetInvalidCastMessages(object valueToConvert, Type resultType)
            string errorId, errorMsg;
            if (resultType.IsByRefLike)
                typeConversion.WriteLine("Cannot convert to ByRef-Like types as they should be used on stack only.");
                errorId = nameof(ExtendedTypeSystem.InvalidCastToByRefLikeType);
                errorMsg = StringUtil.Format(ExtendedTypeSystem.InvalidCastToByRefLikeType, resultType);
                return Tuple.Create(errorId, errorMsg);
            if (PSObject.Base(valueToConvert) == null)
                if (resultType.IsEnum)
                    typeConversion.WriteLine("Issuing an error message about not being able to convert null to an Enum type.");
                    // a nice error message specifically for null being converted to enum
                    errorId = "nullToEnumInvalidCast";
                    errorMsg = StringUtil.Format(ExtendedTypeSystem.InvalidCastExceptionEnumerationNull, resultType,
                                                 EnumSingleTypeConverter.EnumValues(resultType));
                typeConversion.WriteLine("Cannot convert null.");
                // finally throw of all other value types...
                errorId = "nullToObjectInvalidCast";
                errorMsg = StringUtil.Format(ExtendedTypeSystem.InvalidCastFromNull, resultType.ToString());
            typeConversion.WriteLine("Type Conversion failed.");
            errorId = "ConvertToFinalInvalidCastException";
            string valueToConvertTypeName = ObjectToTypeNameString(valueToConvert);
            string resultTypeName = resultType.ToString();
            if (resultType == typeof(SecureString) || resultType == typeof(PSCredential))
                errorMsg = StringUtil.Format(
                    ExtendedTypeSystem.InvalidCastExceptionWithoutValue,
                    valueToConvertTypeName,
                    resultTypeName);
                    valueToConvert.ToString(),
        // Even though this never returns, expression trees expect non-void values in places, and so it's easier
        // to claim it returns object than to add extra expressions to keep the trees type safe.
        internal static object ThrowInvalidCastException(object valueToConvert, Type resultType)
            // Get exception messages (in order): errorId, errorMsg
            var errorMsgTuple = GetInvalidCastMessages(valueToConvert, resultType);
            throw new PSInvalidCastException(errorMsgTuple.Item1, errorMsgTuple.Item2, innerException: null);
        internal static object ThrowInvalidConversionException(object valueToConvert, Type resultType)
            typeConversion.WriteLine("Issuing an error message about not being able to convert to non-core type.");
            throw new PSInvalidCastException("ConversionSupportedOnlyToCoreTypes", null, ExtendedTypeSystem.InvalidCastExceptionNonCoreType, resultType.ToString());
        private static IConversionData FigureLanguageConversion(Type fromType, Type toType,
                                                               out PSConverter<object> valueDependentConversion,
                                                               out ConversionRank valueDependentRank)
            valueDependentConversion = null;
            valueDependentRank = ConversionRank.None;
            Type underlyingType = Nullable.GetUnderlyingType(toType);
            if (underlyingType != null)
                IConversionData nullableConversion = FigureConversion(fromType, underlyingType);
                if (nullableConversion.Rank != ConversionRank.None)
                    return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertToNullable, nullableConversion.Rank);
            if (toType == typeof(void))
                return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertToVoid, ConversionRank.Language);
            if (toType == typeof(bool))
                PSConverter<bool> converter;
                if (typeof(IList).IsAssignableFrom(fromType))
                    converter = LanguagePrimitives.ConvertIListToBool;
                else if (fromType.IsEnum)
                    converter = LanguagePrimitives.CreateNumericToBoolConverter(fromType);
                else if (fromType.IsValueType)
                    converter = LanguagePrimitives.ConvertValueToBool;
                    converter = LanguagePrimitives.ConvertClassToBool;
                return CacheConversion<bool>(fromType, toType, converter, ConversionRank.Language);
            if (toType == typeof(string))
                Dbg.Assert(!LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(fromType)) || fromType.IsEnum,
                    "Number to string should be cached on initialization of cache table");
                return CacheConversion<string>(fromType, toType, LanguagePrimitives.ConvertNonNumericToString, ConversionRank.ToString);
            if (toType.IsArray)
                Type toElementType = toType.GetElementType();
                if (fromType.IsArray)
                    if (toElementType.IsAssignableFrom(fromType.GetElementType()))
                        return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertRelatedArrays, ConversionRank.Language);
                    return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertUnrelatedArrays, ConversionRank.UnrelatedArrays);
                if (LanguagePrimitives.IsTypeEnumerable(fromType))
                    return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertEnumerableToArray, ConversionRank.Language);
                IConversionData data = FigureConversion(fromType, toElementType);
                    valueDependentRank = data.Rank & ConversionRank.ValueDependent;
                    valueDependentConversion = LanguagePrimitives.ConvertScalarToArray;
            if (toType == typeof(Array))
                if (fromType.IsArray || fromType == typeof(Array))
                    return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertAssignableFrom, ConversionRank.Assignable);
                valueDependentRank = ConversionRank.Assignable & ConversionRank.ValueDependent;
            if (toType == typeof(Hashtable))
                if (typeof(IDictionary).IsAssignableFrom(fromType))
                    return CacheConversion<Hashtable>(fromType, toType, LanguagePrimitives.ConvertIDictionaryToHashtable, ConversionRank.Language);
            if (toType == typeof(PSReference))
                return CacheConversion<PSReference>(fromType, toType, LanguagePrimitives.ConvertToPSReference, ConversionRank.Language);
            if (toType == typeof(XmlDocument))
                return CacheConversion<XmlDocument>(fromType, toType, LanguagePrimitives.ConvertToXml, ConversionRank.Language);
            if (toType == typeof(StringCollection))
                ConversionRank rank = (fromType.IsArray || IsTypeEnumerable(fromType)) ? ConversionRank.Language : ConversionRank.LanguageS2A;
                return CacheConversion<StringCollection>(fromType, toType, LanguagePrimitives.ConvertToStringCollection, rank);
            if (toType.IsSubclassOf(typeof(System.Delegate))
                && (fromType == typeof(ScriptBlock) || fromType.IsSubclassOf(typeof(ScriptBlock))))
                return CacheConversion<Delegate>(fromType, toType, LanguagePrimitives.ConvertScriptBlockToDelegate, ConversionRank.Language);
            if (toType == typeof(InternalPSCustomObject))
                Type actualResultType = typeof(PSObject);
                ConstructorInfo resultConstructor = actualResultType.GetConstructor(Type.EmptyTypes);
                var converterObj = new ConvertViaNoArgumentConstructor(resultConstructor, actualResultType);
                return CacheConversion(fromType, toType, converterObj.Convert, ConversionRank.Language);
            TypeCode fromTypeCode = LanguagePrimitives.GetTypeCode(fromType);
            if (LanguagePrimitives.IsInteger(fromTypeCode) && toType.IsEnum)
                return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertIntegerToEnum, ConversionRank.Language);
            if (fromType.IsSubclassOf(typeof(PSMethod)) && toType.IsSubclassOf(typeof(Delegate)) && !toType.IsAbstract)
                var targetMethod = toType.GetMethod("Invoke");
                var comparator = new SignatureComparator(targetMethod);
                var signatureEnumerator = new PSMethodSignatureEnumerator(fromType);
                int index = -1, matchedIndex = -1;
                while (signatureEnumerator.MoveNext())
                    var signatureType = signatureEnumerator.Current;
                    // Skip the non-bindable signatures
                    if (signatureType == typeof(Func<PSNonBindableType>)) { continue; }
                    Type[] argumentTypes = signatureType.GenericTypeArguments;
                    if (comparator.ProjectedSignatureMatchesTarget(argumentTypes, out bool signaturesMatchExactly))
                        if (signaturesMatchExactly)
                            // We prefer the signature that exactly matches the target delegate.
                            matchedIndex = index;
                        // If there is no exact match, then we use the first compatible signature we found.
                        if (matchedIndex == -1) { matchedIndex = index; }
                if (matchedIndex > -1)
                    // We got the index of the matching method signature based on the PSMethod<..> type.
                    // Signatures in PSMethod<..> type were constructed based on the array of method overloads,
                    // in the exact order. So we can use this index directly to locate the matching overload in
                    // the converter, without having to compare the signature again.
                    var converter = PSMethodToDelegateConverter.GetConverter(matchedIndex);
                    return CacheConversion<Delegate>(fromType, toType, converter.Convert, ConversionRank.Language);
        private readonly struct SignatureComparator
            private enum TypeMatchingContext
                ReturnType,
                ParameterType,
                OutParameterType
            private readonly ParameterInfo[] targetParameters;
            private readonly Type targetReturnType;
            internal SignatureComparator(MethodInfo targetMethodInfo)
                targetReturnType = targetMethodInfo.ReturnType;
                targetParameters = targetMethodInfo.GetParameters();
            /// Check if a projected signature matches the target method.
            /// <param name="argumentTypes">
            /// The type arguments from the metadata type 'Func[..]' that represents the projected signature.
            /// It contains the return type as the last item in the array.
            /// <param name="signaturesMatchExactly">
            /// Set by this method to indicate if it's an exact match.
            internal bool ProjectedSignatureMatchesTarget(Type[] argumentTypes, out bool signaturesMatchExactly)
                signaturesMatchExactly = false;
                int length = argumentTypes.Length;
                if (length != targetParameters.Length + 1) { return false; }
                bool typesMatchExactly, allTypesMatchExactly;
                Type sourceReturnType = argumentTypes[length - 1];
                if (ProjectedTypeMatchesTargetType(sourceReturnType, targetReturnType, TypeMatchingContext.ReturnType, out typesMatchExactly))
                    allTypesMatchExactly = typesMatchExactly;
                    for (int i = 0; i < targetParameters.Length; i++)
                        var targetParam = targetParameters[i];
                        var sourceType = argumentTypes[i];
                        var matchContext = targetParam.IsOut ? TypeMatchingContext.OutParameterType : TypeMatchingContext.ParameterType;
                        if (!ProjectedTypeMatchesTargetType(sourceType, targetParam.ParameterType, matchContext, out typesMatchExactly))
                        allTypesMatchExactly &= typesMatchExactly;
                    signaturesMatchExactly = allTypesMatchExactly;
            private static bool ProjectedTypeMatchesTargetType(Type sourceType, Type targetType, TypeMatchingContext matchContext, out bool matchExactly)
                matchExactly = false;
                if (targetType.IsByRef || targetType.IsPointer)
                    if (!sourceType.IsGenericType) { return false; }
                    var sourceTypeDef = sourceType.GetGenericTypeDefinition();
                    bool isOutParameter = matchContext == TypeMatchingContext.OutParameterType;
                    if (targetType.IsByRef && sourceTypeDef == (isOutParameter ? typeof(PSOutParameter<>) : typeof(PSReference<>)) ||
                        targetType.IsPointer && sourceTypeDef == typeof(PSPointer<>))
                        // For ref/out parameter types and pointer types, the element types need to match exactly.
                        if (targetType.GetElementType() == sourceType.GenericTypeArguments[0])
                            matchExactly = true;
                if (targetType == sourceType ||
                    targetType == typeof(void) && sourceType == typeof(VOID) ||
                    targetType == typeof(TypedReference) && sourceType == typeof(PSTypedReference))
                if (targetType == typeof(void) || targetType == typeof(TypedReference))
                return matchContext == TypeMatchingContext.ReturnType
                    ? targetType.IsAssignableFrom(sourceType)
                    : sourceType.IsAssignableFrom(targetType);
        private static PSConverter<object> FigureStaticCreateMethodConversion(Type fromType, Type toType)
            // after discussing this with Jason, we decided that for now we only want to support string->CimSession conversion
            // and we don't want to add a Parse-like conversion based on a static Create method
            if (fromType == typeof(string) && toType == typeof(Microsoft.Management.Infrastructure.CimSession))
                return LanguagePrimitives.ConvertStringToCimSession;
        private static PSConverter<object> FigureParseConversion(Type fromType, Type toType)
            if (toType.IsEnum)
                if (fromType == typeof(string))
                    return LanguagePrimitives.ConvertStringToEnum;
                    return LanguagePrimitives.ConvertEnumerableToEnum;
            else if (fromType == typeof(string))
                const BindingFlags parseFlags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static;
                // GetMethod could throw for more than one match, for instance
                MethodInfo parse = null;
                    parse = toType.GetMethod("Parse", parseFlags, null, new Type[2] { typeof(string), typeof(IFormatProvider) }, null);
                catch (AmbiguousMatchException e)
                    typeConversion.WriteLine("Exception finding Parse method with CultureInfo: \"{0}\".", e.Message);
                if (parse != null)
                    ConvertViaParseMethod converter = new ConvertViaParseMethod();
                    converter.parse = parse;
                    return converter.ConvertWithCulture;
                    parse = toType.GetMethod("Parse", parseFlags, null, new Type[1] { typeof(string) }, null);
                    typeConversion.WriteLine("Exception finding Parse method: \"{0}\".", e.Message);
                    return converter.ConvertWithoutCulture;
        /// Figure conversion when following conditions are satisfied:
        /// 2. fromType is System.Array, System.Object[] or it's the same as the element type of toType.
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        internal static Tuple<PSConverter<object>, ConversionRank> FigureIEnumerableConstructorConversion(Type fromType, Type toType)
            // Win8: 653180. If toType is an Abstract type then we cannot construct it anyway. So, bailing out fast.
            if (toType.IsAbstract)
                bool isScalar = false;
                Type elementType = null;
                ConstructorInfo resultConstructor = null;
                if (toType.IsGenericType && !toType.ContainsGenericParameters &&
                    (typeof(IList).IsAssignableFrom(toType) ||
                     typeof(ICollection).IsAssignableFrom(toType) ||
                     typeof(IEnumerable).IsAssignableFrom(toType)))
                    Type[] argTypes = toType.GetGenericArguments();
                    if (argTypes.Length != 1)
                        typeConversion
                            .WriteLine(
                                "toType has more than one generic arguments. Here we only care about the toType which contains only one generic argument and whose constructor takes IEnumerable<T>, ICollection<T> or IList<T>.");
                    elementType = argTypes[0];
                    if (typeof(Array) == fromType || typeof(object[]) == fromType ||
                        elementType.IsAssignableFrom(fromType) ||
                        // WinBlue: 423899 : To support scenario like [list[int]]"4"
                        (FigureConversion(fromType, elementType) != null))
                        isScalar = elementType.IsAssignableFrom(fromType);
                        ConstructorInfo[] ctors = toType.GetConstructors();
                        Type iEnumerableClosedType = typeof(IEnumerable<>).MakeGenericType(elementType);
                        Type iCollectionClosedType = typeof(ICollection<>).MakeGenericType(elementType);
                        Type iListClosedType = typeof(IList<>).MakeGenericType(elementType);
                        foreach (var ctor in ctors)
                            ParameterInfo[] param = ctor.GetParameters();
                            if (param.Length != 1)
                            Type paramType = param[0].ParameterType;
                            if (iEnumerableClosedType == paramType ||
                                iCollectionClosedType == paramType ||
                                iListClosedType == paramType)
                                resultConstructor = ctor;
                    var converter = new ConvertViaIEnumerableConstructor();
                        Type listClosedType = typeof(List<>).MakeGenericType(elementType);
                        ConstructorInfo listCtor = listClosedType.GetConstructor(new Type[] { typeof(int) });
                        converter.ListCtorLambda = CreateCtorLambdaClosure<int, IList>(listCtor, typeof(int), false);
                        ParameterInfo[] targetParams = resultConstructor.GetParameters();
                        Type targetParamType = targetParams[0].ParameterType;
                        converter.TargetCtorLambda = CreateCtorLambdaClosure<IList, object>(resultConstructor,
                                                                                            targetParamType, false);
                        converter.ElementType = elementType;
                        converter.IsScalar = isScalar;
                        typeConversion.WriteLine("Exception building constructor lambda: \"{0}\"", e.Message);
                    ConversionRank rank = isScalar ? ConversionRank.ConstructorS2A : ConversionRank.Constructor;
                    typeConversion.WriteLine("Conversion is figured out. Conversion rank: \"{0}\"", rank);
                    return new Tuple<PSConverter<object>, ConversionRank>(converter.Convert, rank);
                    typeConversion.WriteLine("Fail to figure out the conversion from \"{0}\" to \"{1}\"",
                                             fromType.FullName, toType.FullName);
                typeConversion.WriteLine("Exception finding IEnumerable conversion: \"{0}\".", ae.Message);
            catch (InvalidOperationException ie)
                typeConversion.WriteLine("Exception finding IEnumerable conversion: \"{0}\".", ie.Message);
            catch (NotSupportedException ne)
                typeConversion.WriteLine("Exception finding IEnumerable conversion: \"{0}\".", ne.Message);
        private static Func<T1, T2> CreateCtorLambdaClosure<T1, T2>(ConstructorInfo ctor, Type realParamType, bool useExplicitConversion)
            ParameterExpression paramExpr = Expression.Parameter(typeof(T1), "args");
            Expression castParamExpr = useExplicitConversion
                ? (Expression)Expression.Call(CachedReflectionInfo.Convert_ChangeType, paramExpr, Expression.Constant(realParamType, typeof(Type)))
                : Expression.Convert(paramExpr, realParamType);
            NewExpression ctorExpr = Expression.New(ctor, castParamExpr.Cast(realParamType));
            return Expression.Lambda<Func<T1, T2>>(ctorExpr.Cast(typeof(T2)), paramExpr).Compile();
        internal static PSConverter<object> FigureConstructorConversion(Type fromType, Type toType)
            if (IsIntegralType(fromType) &&
                (typeof(IList).IsAssignableFrom(toType) || typeof(ICollection).IsAssignableFrom(toType)))
                typeConversion.WriteLine("Ignoring the collection constructor that takes an integer, since this is not semantically a conversion.");
                resultConstructor = toType.GetConstructor(new Type[] { fromType });
                typeConversion.WriteLine("Exception finding Constructor: \"{0}\".", e.Message);
            if (resultConstructor == null)
            typeConversion.WriteLine("Found Constructor.");
            var converter = new ConvertViaConstructor();
                bool useExplicitConversion = targetParamType.IsValueType && fromType != targetParamType && Nullable.GetUnderlyingType(targetParamType) == null;
                converter.TargetCtorLambda = CreateCtorLambdaClosure<object, object>(resultConstructor, targetParamType, useExplicitConversion);
            typeConversion.WriteLine("Conversion is figured out.");
            return converter.Convert;
        private static bool IsIntegralType(Type type)
                type == typeof(sbyte) ||
                type == typeof(byte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong);
        internal static PSConverter<object> FigurePropertyConversion(Type fromType, Type toType, ref ConversionRank rank)
            if ((!typeof(PSObject).IsAssignableFrom(fromType)) || (toType.IsAbstract))
            ConstructorInfo toConstructor = null;
                toConstructor = toType.GetConstructor(Type.EmptyTypes);
            if (toConstructor == null && !toType.IsValueType)
            if (toType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Length == 0 &&
                toType.GetFields(BindingFlags.Public | BindingFlags.Instance).Length == 0)
                // fromType is PSObject, toType has no properties/fields to set, so conversion should fail.
                ConvertViaNoArgumentConstructor noArgumentConstructorConverter =
                    new ConvertViaNoArgumentConstructor(toConstructor, toType);
                rank = ConversionRank.Constructor;
                return noArgumentConstructorConverter.Convert;
                typeConversion.WriteLine("Exception converting via no argument constructor: \"{0}\".", ae.Message);
                typeConversion.WriteLine("Exception converting via no argument constructor: \"{0}\".", ie.Message);
            rank = ConversionRank.None;
        internal static PSConverter<object> FigureCastConversion(Type fromType, Type toType, ref ConversionRank rank)
            MethodInfo castOperator = FindCastOperator("op_Implicit", toType, fromType, toType);
            if (castOperator == null)
                castOperator = FindCastOperator("op_Explicit", toType, fromType, toType);
                    castOperator = FindCastOperator("op_Implicit", fromType, fromType, toType) ??
                                   FindCastOperator("op_Explicit", fromType, fromType, toType);
            if (castOperator != null)
                rank = castOperator.Name.Equals("op_Implicit", StringComparison.OrdinalIgnoreCase)
                    ? ConversionRank.ImplicitCast : ConversionRank.ExplicitCast;
                ConvertViaCast converter = new ConvertViaCast();
                converter.cast = castOperator;
        private static bool TypeConverterPossiblyExists(Type type)
            lock (s_possibleTypeConverter)
                if (s_possibleTypeConverter.ContainsKey(type.FullName))
            // GetCustomAttributes returns IEnumerable<Attribute> in CoreCLR
            if (typeConverters.Length > 0)
        private static readonly Dictionary<string, bool> s_possibleTypeConverter = new Dictionary<string, bool>(16);
        // This is the internal dummy type used when an IDictionary is converted to a pscustomobject
        // PS C:\> $ps = [pscustomobject]@{a=10;b=5}
        // PS C:\> $ps = [pscustomobject][ordered]@{a=10;b=5}
        // Whenever we see a conversion to PSCustomObject, we represent it as a conversion to InternalPSCustomObject
        // This is introduced to avoid breaking PSObject behavior.
        // (Because PSCustomObject is a typeaccelerator for PSObject, we needed a separate type to represent type conversions to PSCustomObject)
        internal class InternalPSCustomObject
        internal class InternalPSObject : PSObject { }
        internal static IConversionData FigureConversion(Type fromType, Type toType)
            IConversionData data = GetConversionData(fromType, toType);
            if (fromType == typeof(Null))
                return FigureConversionFromNull(toType);
            if (toType.IsAssignableFrom(fromType))
                return CacheConversion<object>(fromType, toType, LanguagePrimitives.ConvertAssignableFrom,
                                               toType == fromType ? ConversionRank.Identity : ConversionRank.Assignable);
            if (fromType.IsByRefLike || toType.IsByRefLike)
                // ByRef-like types are not boxable and should be used on stack only.
                return CacheConversion(fromType, toType, ConvertNoConversion, ConversionRank.None);
            if (typeof(PSObject).IsAssignableFrom(fromType) && typeof(InternalPSObject) != fromType)
                // We don't attempt converting PSObject (or derived) to anything else,
                // instead we go straight to PSObject.Base (which is only a PSObject
                // when no object is wrapped, in which case we try conversions from object)
                // and convert that instead.
            if (toType == typeof(PSObject))
                return CacheConversion<PSObject>(fromType, toType, LanguagePrimitives.ConvertToPSObject, ConversionRank.PSObject);
            PSConverter<object> converter = null;
            // If we've ever used ConstrainedLanguage, check if the target type is allowed.
            if (ExecutionContext.HasEverUsedConstrainedLanguage)
                if (context?.LanguageMode == PSLanguageMode.ConstrainedLanguage)
                    if (toType != typeof(object) &&
                        toType != typeof(object[]) &&
                        !CoreTypes.Contains(toType))
                            converter = ConvertNotSupportedConversion;
                            return CacheConversion(fromType, toType, converter, rank);
                            title: ExtendedTypeSystem.WDACTypeConversionLogTitle,
                            message: StringUtil.Format(ExtendedTypeSystem.WDACTypeConversionLogMessage, fromType.FullName, toType.FullName),
                            fqid: "LanguageTypeConversionNotAllowed",
            PSConverter<object> valueDependentConversion = null;
            ConversionRank valueDependentRank = ConversionRank.None;
            IConversionData conversionData = FigureLanguageConversion(fromType, toType, out valueDependentConversion, out valueDependentRank);
            if (conversionData != null)
                return conversionData;
            rank = valueDependentConversion != null ? ConversionRank.Language : ConversionRank.None;
            converter = FigureParseConversion(fromType, toType);
                converter = FigureStaticCreateMethodConversion(fromType, toType);
                    converter = FigureConstructorConversion(fromType, toType);
                        converter = FigureCastConversion(fromType, toType, ref rank);
                            if (typeof(IConvertible).IsAssignableFrom(fromType))
                                if (LanguagePrimitives.IsNumeric(GetTypeCode(fromType)) && !fromType.IsEnum)
                                    if (!toType.IsArray)
                                        if (GetConversionRank(typeof(string), toType) != ConversionRank.None)
                                            converter = LanguagePrimitives.ConvertNumericIConvertible;
                                            rank = ConversionRank.IConvertible;
                                else if (fromType != typeof(string))
                                    converter = LanguagePrimitives.ConvertIConvertible;
                            else if (typeof(IDictionary).IsAssignableFrom(fromType))
                                // We need to call the null argument constructor only if the following 2 conditions satisfy
                                //  1) if the fromType is either a hashtable or OrderedDictionary
                                //  2) if the ToType does not already have a constructor that takes a hashtable or OrderedDictionary. (This is to avoid breaking existing apps.)
                                // If the ToType has a constructor that takes a hashtable or OrderedDictionary,
                                // then it would have been returned as the constructor during FigureConstructorConversion
                                // So, we need to check only for the first condition
                                ConstructorInfo resultConstructor = toType.GetConstructor(Type.EmptyTypes);
                                if (resultConstructor != null || (toType.IsValueType && !toType.IsPrimitive))
                                    ConvertViaNoArgumentConstructor noArgumentConstructorConverter = new ConvertViaNoArgumentConstructor(resultConstructor, toType);
                                    converter = noArgumentConstructorConverter.Convert;
                    rank = ConversionRank.Create;
                rank = ConversionRank.Parse;
                var tuple = FigureIEnumerableConstructorConversion(fromType, toType);
                if (tuple != null)
                    converter = tuple.Item1;
                    rank = tuple.Item2;
            converter ??= FigurePropertyConversion(fromType, toType, ref rank);
            if (TypeConverterPossiblyExists(fromType) || TypeConverterPossiblyExists(toType)
                || (converter != null && valueDependentConversion != null))
                ConvertCheckingForCustomConverter customConverter = new ConvertCheckingForCustomConverter();
                customConverter.tryfirstConverter = valueDependentConversion;
                customConverter.fallbackConverter = converter;
                converter = customConverter.Convert;
                if (valueDependentRank > rank)
                    rank = valueDependentRank;
                else if (rank == ConversionRank.None)
                    rank = ConversionRank.Custom;
            else if (valueDependentConversion != null)
                converter = valueDependentConversion;
                converter = ConvertNoConversion;
        internal class Null { }
        private static IConversionData FigureConversionFromNull(Type toType)
            IConversionData data = GetConversionData(typeof(Null), toType);
            if (Nullable.GetUnderlyingType(toType) != null)
                return CacheConversion<object>(typeof(Null), toType, LanguagePrimitives.ConvertNullToNullable, ConversionRank.NullToValue);
            else if (!toType.IsValueType)
                return CacheConversion<object>(typeof(Null), toType, LanguagePrimitives.ConvertNullToRef, ConversionRank.NullToRef);
            return CacheConversion(typeof(Null), toType, ConvertNoConversion, ConversionRank.None);
        internal static string ObjectToTypeNameString(object o)
                return "null";
            var typeNames = pso.InternalTypeNames;
            if ((typeNames != null) && (typeNames.Count > 0))
            return Microsoft.PowerShell.ToStringCodeMethods.Type(o.GetType());
        private static Assembly AssemblyResolveHelper(object sender, ResolveEventArgs args)
            // Dynamic assemblies don't get resolved properly by the CLR, so we
            // resolve them ourselves by looking through the loaded assemblies.
            foreach (Assembly assem in ClrFacade.GetAssemblies())
                if (assem.FullName == args.Name)
                    return assem;
        #endregion type converter

using System.DirectoryServices;
    /// Contains auxiliary ToString CodeMethod implementations for some types.
    public static partial class ToStringCodeMethods
        /// ToString implementation for PropertyValueCollection.
        /// <param name="instance">Instance of PSObject wrapping a PropertyValueCollection.</param>
        public static string PropertyValueCollection(PSObject instance)
            var values = (PropertyValueCollection)instance.BaseObject;
            if (values.Count == 1)
                if (values[0] == null)
                return (PSObject.AsPSObject(values[0]).ToString());
            return PSObject.ToStringEnumerable(null, (IEnumerable)values, null, null, null);
    /// Contains CodeMethod implementations for some adapted types like:
    /// 1. DirectoryEntry Related Code Methods
    ///    (a) Convert from DE LargeInteger to Int64.
    ///    (b) Convert from DE Dn-With-Binary to string.
    public static class AdapterCodeMethods
        #region DirectoryEntry related CodeMethods
        /// Converts instance of LargeInteger to .net Int64.
        /// <param name="deInstance">Instance of PSObject wrapping DirectoryEntry object.</param>
        /// <param name="largeIntegerInstance">Instance of PSObject wrapping LargeInteger instance.</param>
        /// <returns>Converted Int64.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer")]
        public static Int64 ConvertLargeIntegerToInt64(PSObject deInstance, PSObject largeIntegerInstance)
            if (largeIntegerInstance == null)
                throw PSTraceSource.NewArgumentException(nameof(largeIntegerInstance));
            object largeIntObject = (object)largeIntegerInstance.BaseObject;
            Type largeIntType = largeIntObject.GetType();
            // the following code might throw exceptions,
            // engine will catch these exceptions
            int highPart = (int)largeIntType.InvokeMember("HighPart",
                BindingFlags.GetProperty | BindingFlags.Public,
                largeIntObject,
            int lowPart = (int)largeIntType.InvokeMember("LowPart",
            // LowPart is not really a signed integer. Do not try to
            // use LowPart as a signed integer or you may get intermittent
            // surprises.
            // (long)highPart << 32 | (uint)lowPart
            byte[] data = new byte[8];
            BitConverter.GetBytes(lowPart).CopyTo(data, 0);
            BitConverter.GetBytes(highPart).CopyTo(data, 4);
            return BitConverter.ToInt64(data, 0);
        /// Converts instance of DN-With-Binary to .net String.
        /// <param name="dnWithBinaryInstance">Instance of PSObject wrapping DN-With-Binary object.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dn", Justification = "DN represents valid prefix w.r.t Active Directory.")]
        public static string ConvertDNWithBinaryToString(PSObject deInstance, PSObject dnWithBinaryInstance)
            if (dnWithBinaryInstance == null)
                throw PSTraceSource.NewArgumentException(nameof(dnWithBinaryInstance));
            object dnWithBinaryObject = (object)dnWithBinaryInstance.BaseObject;
            Type dnWithBinaryType = dnWithBinaryObject.GetType();
            string dnString = (string)dnWithBinaryType.InvokeMember("DNString",
                dnWithBinaryObject,
            return dnString;

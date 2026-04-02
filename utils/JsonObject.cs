using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
    /// JsonObject class.
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Preferring Json over JSON")]
    public static class JsonObject
        #region HelperTypes
        /// Context for convert-to-json operation.
        public readonly struct ConvertToJsonContext
            /// Gets the maximum depth for walking the object graph.
            public readonly int MaxDepth;
            /// Gets the cancellation token.
            public readonly CancellationToken CancellationToken;
            /// Gets the StringEscapeHandling setting.
            public readonly StringEscapeHandling StringEscapeHandling;
            /// Gets the EnumsAsStrings setting.
            public readonly bool EnumsAsStrings;
            /// Gets the CompressOutput setting.
            public readonly bool CompressOutput;
            /// Gets the target cmdlet that is doing the convert-to-json operation.
            public readonly PSCmdlet Cmdlet;
            /// Initializes a new instance of the <see cref="ConvertToJsonContext"/> struct.
            /// <param name="maxDepth">The maximum depth to visit the object.</param>
            /// <param name="enumsAsStrings">Indicates whether to use enum names for the JSON conversion.</param>
            /// <param name="compressOutput">Indicates whether to get the compressed output.</param>
            public ConvertToJsonContext(int maxDepth, bool enumsAsStrings, bool compressOutput)
                : this(maxDepth, enumsAsStrings, compressOutput, StringEscapeHandling.Default, targetCmdlet: null, CancellationToken.None)
            /// <param name="stringEscapeHandling">Specifies how strings are escaped when writing JSON text.</param>
            /// <param name="targetCmdlet">Specifies the cmdlet that is calling this method.</param>
            /// <param name="cancellationToken">Specifies the cancellation token for cancelling the operation.</param>
            public ConvertToJsonContext(
                int maxDepth,
                bool enumsAsStrings,
                bool compressOutput,
                StringEscapeHandling stringEscapeHandling,
                PSCmdlet targetCmdlet,
                CancellationToken cancellationToken)
                this.MaxDepth = maxDepth;
                this.CancellationToken = cancellationToken;
                this.StringEscapeHandling = stringEscapeHandling;
                this.EnumsAsStrings = enumsAsStrings;
                this.CompressOutput = compressOutput;
                this.Cmdlet = targetCmdlet;
        private sealed class DuplicateMemberHashSet : HashSet<string>
            public DuplicateMemberHashSet(int capacity)
                : base(capacity, StringComparer.OrdinalIgnoreCase)
        #endregion HelperTypes
        #region ConvertFromJson
        /// Convert a Json string back to an object of type PSObject.
        /// <param name="input">The json text to convert.</param>
        /// <param name="error">An error record if the conversion failed.</param>
        /// <returns>A PSObject.</returns>
        public static object ConvertFromJson(string input, out ErrorRecord error)
            return ConvertFromJson(input, returnHashtable: false, out error);
        /// Convert a Json string back to an object of type <see cref="System.Management.Automation.PSObject"/> or
        /// <see cref="System.Collections.Hashtable"/> depending on parameter <paramref name="returnHashtable"/>.
        /// <param name="returnHashtable">True if the result should be returned as a <see cref="System.Collections.Hashtable"/>
        /// instead of a <see cref="System.Management.Automation.PSObject"/></param>
        /// <returns>A <see cref="System.Management.Automation.PSObject"/> or a <see cref="System.Collections.Hashtable"/>
        /// if the <paramref name="returnHashtable"/> parameter is true.</returns>
        public static object ConvertFromJson(string input, bool returnHashtable, out ErrorRecord error)
            return ConvertFromJson(input, returnHashtable, maxDepth: 1024, out error);
        /// Convert a JSON string back to an object of type <see cref="System.Management.Automation.PSObject"/> or
        /// <param name="input">The JSON text to convert.</param>
        /// instead of a <see cref="System.Management.Automation.PSObject"/>.</param>
        /// <param name="maxDepth">The max depth allowed when deserializing the json input. Set to null for no maximum.</param>
        public static object ConvertFromJson(string input, bool returnHashtable, int? maxDepth, out ErrorRecord error)
            => ConvertFromJson(input, returnHashtable, maxDepth, jsonDateKind: JsonDateKind.Default, out error);
        /// <param name="jsonDateKind">Controls how DateTime values are to be converted.</param>
        internal static object ConvertFromJson(string input, bool returnHashtable, int? maxDepth, JsonDateKind jsonDateKind, out ErrorRecord error)
            ArgumentNullException.ThrowIfNull(input);
            DateParseHandling dateParseHandling;
            DateTimeZoneHandling dateTimeZoneHandling;
            switch (jsonDateKind)
                case JsonDateKind.Default:
                    dateParseHandling = DateParseHandling.DateTime;
                    dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                case JsonDateKind.Local:
                    dateTimeZoneHandling = DateTimeZoneHandling.Local;
                case JsonDateKind.Utc:
                    dateTimeZoneHandling = DateTimeZoneHandling.Utc;
                case JsonDateKind.Offset:
                    dateParseHandling = DateParseHandling.DateTimeOffset;
                    dateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                case JsonDateKind.String:
                    dateParseHandling = DateParseHandling.None;
                    throw new ArgumentException($"Unknown JsonDateKind value requested '{jsonDateKind}'");
            error = null;
                var obj = JsonConvert.DeserializeObject(
                    input,
                    new JsonSerializerSettings
                        DateParseHandling = dateParseHandling,
                        DateTimeZoneHandling = dateTimeZoneHandling,
                        // This TypeNameHandling setting is required to be secure.
                        TypeNameHandling = TypeNameHandling.None,
                        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                        MaxDepth = maxDepth
                switch (obj)
                    case JObject dictionary:
                        // JObject is a IDictionary
                        return returnHashtable
                                   ? PopulateHashTableFromJDictionary(dictionary, out error)
                                   : PopulateFromJDictionary(dictionary, new DuplicateMemberHashSet(dictionary.Count), out error);
                    case JArray list:
                                   ? PopulateHashTableFromJArray(list, out error)
                                   : PopulateFromJArray(list, out error);
            catch (JsonException je)
                var msg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.JsonDeserializationFailed, je.Message);
                // the same as JavaScriptSerializer does
                throw new ArgumentException(msg, je);
        // This function is a clone of PopulateFromDictionary using JObject as an input.
        private static PSObject PopulateFromJDictionary(JObject entries, DuplicateMemberHashSet memberHashTracker, out ErrorRecord error)
            var result = new PSObject(entries.Count);
            foreach (var entry in entries)
                if (string.IsNullOrEmpty(entry.Key))
                    var errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.EmptyKeyInJsonString);
                        "EmptyKeyInJsonString",
                // Case sensitive duplicates should normally not occur since JsonConvert.DeserializeObject
                // does not throw when encountering duplicates and just uses the last entry.
                if (memberHashTracker.TryGetValue(entry.Key, out var maybePropertyName)
                    && string.Equals(entry.Key, maybePropertyName, StringComparison.Ordinal))
                    var errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.DuplicateKeysInJsonString, entry.Key);
                        "DuplicateKeysInJsonString",
                // Compare case insensitive to tell the user to use the -AsHashTable option instead.
                // This is because PSObject cannot have keys with different casing.
                if (memberHashTracker.TryGetValue(entry.Key, out var propertyName))
                    var errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.KeysWithDifferentCasingInJsonString, propertyName, entry.Key);
                        "KeysWithDifferentCasingInJsonString",
                switch (entry.Value)
                            var listResult = PopulateFromJArray(list, out error);
                            result.Properties.Add(new PSNoteProperty(entry.Key, listResult));
                    case JObject dic:
                            // Dictionary
                            var dicResult = PopulateFromJDictionary(dic, new DuplicateMemberHashSet(dic.Count), out error);
                            result.Properties.Add(new PSNoteProperty(entry.Key, dicResult));
                    case JValue value:
                            result.Properties.Add(new PSNoteProperty(entry.Key, value.Value));
                memberHashTracker.Add(entry.Key);
        // This function is a clone of PopulateFromList using JArray as input.
        private static ICollection<object> PopulateFromJArray(JArray list, out ErrorRecord error)
            var result = new object[list.Count];
            var i = 0;
            foreach (var element in list)
                switch (element)
                    case JArray subList:
                        result[i++] = PopulateFromJArray(subList, out error);
                        result[i++] = PopulateFromJDictionary(dic, new DuplicateMemberHashSet(dic.Count), out error);
                        if (value.Type != JTokenType.Comment)
                            result[i++] = value.Value;
            // In the common case of not having any comments, return the original array, otherwise create a sliced copy.
            return i == list.Count ? result : result[..i];
        private static Hashtable PopulateHashTableFromJDictionary(JObject entries, out ErrorRecord error)
            OrderedHashtable result = new(entries.Count);
                if (result.ContainsKey(entry.Key))
                    string errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.DuplicateKeysInJsonString, entry.Key);
                            var listResult = PopulateHashTableFromJArray(list, out error);
                            result.Add(entry.Key, listResult);
                            var dicResult = PopulateHashTableFromJDictionary(dic, out error);
                            result.Add(entry.Key, dicResult);
                            result.Add(entry.Key, value.Value);
        private static ICollection<object> PopulateHashTableFromJArray(JArray list, out ErrorRecord error)
                        result[i++] = PopulateHashTableFromJArray(subList, out error);
                        result[i++] = PopulateHashTableFromJDictionary(dic, out error);
        #endregion ConvertFromJson
        #region ConvertToJson
        /// Convert an object to JSON string.
        public static string ConvertToJson(object objectToProcess, in ConvertToJsonContext context)
                // Pre-process the object so that it serializes the same, except that properties whose
                // values cannot be evaluated are treated as having the value null.
                _maxDepthWarningWritten = false;
                object preprocessedObject = ProcessValue(objectToProcess, currentDepth: 0, in context);
                var jsonSettings = new JsonSerializerSettings
                    MaxDepth = 1024,
                    StringEscapeHandling = context.StringEscapeHandling
                if (context.EnumsAsStrings)
                    jsonSettings.Converters.Add(new StringEnumConverter());
                if (!context.CompressOutput)
                    jsonSettings.Formatting = Formatting.Indented;
                return JsonConvert.SerializeObject(preprocessedObject, jsonSettings);
            catch (OperationCanceledException)
        private static bool _maxDepthWarningWritten;
        /// Return an alternate representation of the specified object that serializes the same JSON, except
        /// that properties that cannot be evaluated are treated as having the value null.
        /// Primitive types are returned verbatim.  Aggregate types are processed recursively.
        /// <param name="obj">The object to be processed.</param>
        /// <param name="currentDepth">The current depth into the object graph.</param>
        /// <param name="context">The context to use for the convert-to-json operation.</param>
        /// <returns>An object suitable for serializing to JSON.</returns>
        private static object ProcessValue(object obj, int currentDepth, in ConvertToJsonContext context)
            context.CancellationToken.ThrowIfCancellationRequested();
            if (LanguagePrimitives.IsNull(obj))
            PSObject pso = obj as PSObject;
            if (pso != null)
                obj = pso.BaseObject;
            object rv = obj;
            bool isPurePSObj = false;
            bool isCustomObj = false;
            if (obj == NullString.Value
                || obj == DBNull.Value)
                rv = null;
            else if (obj is string
                    || obj is char
                    || obj is bool
                    || obj is DateTime
                    || obj is DateTimeOffset
                    || obj is Guid
                    || obj is Uri
                    || obj is double
                    || obj is float
                    || obj is decimal
                    || obj is BigInteger)
                rv = obj;
            else if (obj is Newtonsoft.Json.Linq.JObject jObject)
                rv = jObject.ToObject<Dictionary<object, object>>();
                Type t = obj.GetType();
                if (t.IsPrimitive || (t.IsEnum && ExperimentalFeature.IsEnabled(ExperimentalFeature.PSSerializeJSONLongEnumAsNumber)))
                    // Win8:378368 Enums based on System.Int64 or System.UInt64 are not JSON-serializable
                    // because JavaScript does not support the necessary precision.
                    Type enumUnderlyingType = Enum.GetUnderlyingType(obj.GetType());
                    if (enumUnderlyingType.Equals(typeof(long)) || enumUnderlyingType.Equals(typeof(ulong)))
                        rv = obj.ToString();
                    if (currentDepth > context.MaxDepth)
                        if (!_maxDepthWarningWritten && context.Cmdlet != null)
                            _maxDepthWarningWritten = true;
                            string maxDepthMessage = string.Format(
                                WebCmdletStrings.JsonMaxDepthReached,
                                context.MaxDepth);
                            context.Cmdlet.WriteWarning(maxDepthMessage);
                        if (pso != null && pso.ImmediateBaseObjectIsEmpty)
                            // The obj is a pure PSObject, we convert the original PSObject to a string,
                            // instead of its base object in this case
                            rv = LanguagePrimitives.ConvertTo(pso, typeof(string),
                            isPurePSObj = true;
                            rv = LanguagePrimitives.ConvertTo(obj, typeof(string),
                        if (obj is IDictionary dict)
                            rv = ProcessDictionary(dict, currentDepth, in context);
                            if (obj is IEnumerable enumerable)
                                rv = ProcessEnumerable(enumerable, currentDepth, in context);
                                rv = ProcessCustomObject<JsonIgnoreAttribute>(obj, currentDepth, in context);
                                isCustomObj = true;
            rv = AddPsProperties(pso, rv, currentDepth, isPurePSObj, isCustomObj, in context);
        /// Add to a base object any properties that might have been added to an object (via PSObject) through the Add-Member cmdlet.
        /// <param name="psObj">The containing PSObject, or null if the base object was not contained in a PSObject.</param>
        /// <param name="obj">The base object that might have been decorated with additional properties.</param>
        /// <param name="depth">The current depth into the object graph.</param>
        /// <param name="isPurePSObj">The processed object is a pure PSObject.</param>
        /// <param name="isCustomObj">The processed object is a custom object.</param>
        /// <param name="context">The context for the operation.</param>
        /// The original base object if no additional properties had been added,
        /// otherwise a dictionary containing the value of the original base object in the "value" key
        /// as well as the names and values of an additional properties.
        private static object AddPsProperties(object psObj, object obj, int depth, bool isPurePSObj, bool isCustomObj, in ConvertToJsonContext context)
            if (psObj is not PSObject pso)
            // when isPurePSObj is true, the obj is guaranteed to be a string converted by LanguagePrimitives
            if (isPurePSObj)
            bool wasDictionary = true;
            if (obj is not IDictionary dict)
                wasDictionary = false;
                dict = new Dictionary<string, object>();
                dict.Add("value", obj);
            AppendPsProperties(pso, dict, depth, isCustomObj, in context);
            if (!wasDictionary && dict.Count == 1)
            return dict;
        /// Append to a dictionary any properties that might have been added to an object (via PSObject) through the Add-Member cmdlet.
        /// If the passed in object is a custom object (not a simple object, not a dictionary, not a list, get processed in ProcessCustomObject method),
        /// we also take Adapted properties into account. Otherwise, we only consider the Extended properties.
        /// When the object is a pure PSObject, it also gets processed in "ProcessCustomObject" before reaching this method, so we will
        /// iterate both extended and adapted properties for it. Since it's a pure PSObject, there will be no adapted properties.
        /// <param name="receiver">The dictionary to which any additional properties will be appended.</param>
        /// <param name="isCustomObject">The processed object is a custom object.</param>
        private static void AppendPsProperties(PSObject psObj, IDictionary receiver, int depth, bool isCustomObject, in ConvertToJsonContext context)
            // if the psObj is a DateTime or String type, we don't serialize any extended or adapted properties
            if (psObj.BaseObject is string || psObj.BaseObject is DateTime)
            // serialize only Extended and Adapted properties..
                new PSMemberInfoIntegratingCollection<PSPropertyInfo>(psObj,
                    isCustomObject ? PSObject.GetPropertyCollection(PSMemberViewTypes.Extended | PSMemberViewTypes.Adapted) :
                if (!receiver.Contains(prop.Name))
                    receiver[prop.Name] = ProcessValue(value, depth + 1, in context);
        /// Return an alternate representation of the specified dictionary that serializes the same JSON, except
        /// that any contained properties that cannot be evaluated are treated as having the value null.
        private static object ProcessDictionary(IDictionary dict, int depth, in ConvertToJsonContext context)
            Dictionary<string, object> result = new(dict.Count);
            foreach (DictionaryEntry entry in dict)
                string name = entry.Key as string;
                    // use the error string that matches the message from JavaScriptSerializer
                    string errorMsg = string.Format(
                        WebCmdletStrings.NonStringKeyInDictionary,
                        dict.GetType().FullName);
                    var exception = new InvalidOperationException(errorMsg);
                    if (context.Cmdlet != null)
                        var errorRecord = new ErrorRecord(exception, "NonStringKeyInDictionary", ErrorCategory.InvalidOperation, dict);
                        context.Cmdlet.ThrowTerminatingError(errorRecord);
                result.Add(name, ProcessValue(entry.Value, depth + 1, in context));
        /// Return an alternate representation of the specified collection that serializes the same JSON, except
        private static object ProcessEnumerable(IEnumerable enumerable, int depth, in ConvertToJsonContext context)
            List<object> result = new();
                result.Add(ProcessValue(o, depth + 1, in context));
        /// Return an alternate representation of the specified aggregate object that serializes the same JSON, except
        /// The result is a dictionary in which all public fields and public gettable properties of the original object
        /// are represented.  If any exception occurs while retrieving the value of a field or property, that entity
        /// is included in the output dictionary with a value of null.
        private static object ProcessCustomObject<T>(object o, int depth, in ConvertToJsonContext context)
            Dictionary<string, object> result = new();
            Type t = o.GetType();
            foreach (FieldInfo info in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (!info.IsDefined(typeof(T), true))
                    object value;
                        value = info.GetValue(o);
                        value = null;
                    result.Add(info.Name, ProcessValue(value, depth + 1, in context));
            foreach (PropertyInfo info2 in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (!info2.IsDefined(typeof(T), true))
                    MethodInfo getMethod = info2.GetGetMethod();
                    if ((getMethod != null) && (getMethod.GetParameters().Length == 0))
                            value = getMethod.Invoke(o, Array.Empty<object>());
                        result.Add(info2.Name, ProcessValue(value, depth + 1, in context));
        #endregion ConvertToJson

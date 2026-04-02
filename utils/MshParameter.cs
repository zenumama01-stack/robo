    /// Normalized parameter class to be constructed from the command line parameters
    /// using the metadata information provided by an instance of CommandParameterDefinition
    /// it's basically the hash table with the normalized values.
    internal class MshParameter
        internal Hashtable hash = null;
        internal object GetEntry(string key)
            if (this.hash.ContainsKey(key))
                return this.hash[key];
            return AutomationNull.Value;
    internal class NameEntryDefinition : HashtableEntryDefinition
        internal const string NameEntryKey = "name";
        internal NameEntryDefinition()
            : base(NameEntryKey, new string[] { FormatParameterDefinitionKeys.LabelEntryKey }, new Type[] { typeof(string) }, false)
    /// Metadata base class for hashtable entry definitions
    /// it contains the key name and the allowable types
    /// it also provides hooks for type expansion.
    internal class HashtableEntryDefinition
        internal HashtableEntryDefinition(string name, IEnumerable<string> secondaryNames, Type[] types, bool mandatory)
            : this(name, types, mandatory)
            SecondaryNames = secondaryNames;
        internal HashtableEntryDefinition(string name, Type[] types, bool mandatory)
            KeyName = name;
            AllowedTypes = types;
            Mandatory = mandatory;
        internal HashtableEntryDefinition(string name, Type[] types)
            : this(name, types, false)
        internal virtual Hashtable CreateHashtableFromSingleType(object val)
            // NOTE: must override for the default type(s) entry
            // this entry will have to expand the object into a hash table
        internal bool IsKeyMatch(string key)
            if (CommandParameterDefinition.FindPartialMatch(key, this.KeyName))
            if (this.SecondaryNames != null)
                foreach (string secondaryKey in this.SecondaryNames)
                    if (CommandParameterDefinition.FindPartialMatch(key, secondaryKey))
        internal virtual object Verify(object val,
        internal virtual object ComputeDefaultValue()
        internal string KeyName { get; }
        internal Type[] AllowedTypes { get; }
        internal bool Mandatory { get; }
        internal IEnumerable<string> SecondaryNames { get; }
    /// Metadata abstract base class to contain hash entries definitions.
    internal abstract class CommandParameterDefinition
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal CommandParameterDefinition()
            SetEntries();
        protected abstract void SetEntries();
        internal virtual MshParameter CreateInstance() { return new MshParameter(); }
        /// For a key name, verify it is a legal entry:
        ///     1. it must match (partial match allowed)
        ///     2. it must be unambiguous (if partial match)
        /// If an error condition occurs, an exception will be thrown.
        /// <param name="keyName">Key to verify.</param>
        /// <param name="invocationContext">Invocation context for error reporting.</param>
        /// <returns>Matching hash table entry.</returns>
        /// <exception cref="ArgumentException"></exception>
        internal HashtableEntryDefinition MatchEntry(string keyName, TerminatingErrorContext invocationContext)
            if (string.IsNullOrEmpty(keyName))
                PSTraceSource.NewArgumentNullException(nameof(keyName));
            HashtableEntryDefinition matchingEntry = null;
            for (int k = 0; k < this.hashEntries.Count; k++)
                if (this.hashEntries[k].IsKeyMatch(keyName))
                    // we have a match
                    if (matchingEntry == null)
                        // this is the first match, we save the entry
                        // and we keep going for ambiguity check
                        matchingEntry = this.hashEntries[k];
                        // we already had a match, we have an ambiguous key
                        ProcessAmbiguousKey(invocationContext, keyName, matchingEntry, this.hashEntries[k]);
                // we found an unambiguous match
                return matchingEntry;
            // we did not have a match
            ProcessIllegalKey(invocationContext, keyName);
        internal static bool FindPartialMatch(string key, string normalizedKey)
            if (key.Length < normalizedKey.Length)
                // shorter, could be an abbreviation
                if (key.AsSpan().Equals(normalizedKey.AsSpan(0, key.Length), StringComparison.OrdinalIgnoreCase))
                    // found abbreviation
            if (string.Equals(key, normalizedKey, StringComparison.OrdinalIgnoreCase))
                // found full match
        private static void ProcessAmbiguousKey(TerminatingErrorContext invocationContext,
                                            string keyName,
                                            HashtableEntryDefinition matchingEntry,
                                            HashtableEntryDefinition currentEntry)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.AmbiguousKeyError,
                keyName, matchingEntry.KeyName, currentEntry.KeyName);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyAmbiguous", msg);
        private static void ProcessIllegalKey(TerminatingErrorContext invocationContext,
                                            string keyName)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.IllegalKeyError, keyName);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyIllegal", msg);
        internal List<HashtableEntryDefinition> hashEntries = new List<HashtableEntryDefinition>();
    /// Engine to process a generic object[] from the command line and
    /// generate a list of MshParameter objects , given the metadata provided by
    /// a class derived from CommandParameterDefinition.
    internal sealed class ParameterProcessor
        [TraceSource("ParameterProcessor", "ParameterProcessor")]
        internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ParameterProcessor", "ParameterProcessor");
        internal static void ThrowParameterBindingException(TerminatingErrorContext invocationContext,
                                                            string msg)
                                new NotSupportedException(),
            invocationContext.ThrowTerminatingError(errorRecord);
        internal ParameterProcessor(CommandParameterDefinition p)
            _paramDef = p;
        internal List<MshParameter> ProcessParameters(object[] p, TerminatingErrorContext invocationContext)
            if (p == null || p.Length == 0)
            List<MshParameter> retVal = new List<MshParameter>();
            MshParameter currParam;
            bool originalParameterWasHashTable = false;
            for (int k = 0; k < p.Length; k++)
                // we always copy into a fresh hash table
                currParam = _paramDef.CreateInstance();
                var actualObject = PSObject.Base(p[k]);
                if (actualObject is IDictionary)
                    originalParameterWasHashTable = true;
                    currParam.hash = VerifyHashTable((IDictionary)actualObject, invocationContext);
                else if ((actualObject != null) && MatchesAllowedTypes(actualObject.GetType(), _paramDef.hashEntries[0].AllowedTypes))
                    // a simple type was specified, build a hash with one entry
                    currParam.hash = _paramDef.hashEntries[0].CreateHashtableFromSingleType(actualObject);
                    // unknown type, error
                    // provide error message (the user did not enter a hash table)
                    ProcessUnknownParameterType(invocationContext, actualObject, _paramDef.hashEntries[0].AllowedTypes);
                // value range validation and post processing on the hash table
                VerifyAndNormalizeParameter(currParam, invocationContext, originalParameterWasHashTable);
                retVal.Add(currParam);
        private static bool MatchesAllowedTypes(Type t, Type[] allowedTypes)
            for (int k = 0; k < allowedTypes.Length; k++)
                if (allowedTypes[k].IsAssignableFrom(t))
        private Hashtable VerifyHashTable(IDictionary hash, TerminatingErrorContext invocationContext)
            // full blown hash, need to:
            // 1. verify names(keys) and expand names if there are partial matches
            // 2. verify value types
            Hashtable retVal = new Hashtable();
            foreach (DictionaryEntry e in hash)
                if (e.Key == null)
                    ProcessNullHashTableKey(invocationContext);
                string currentStringKey = e.Key as string;
                if (currentStringKey == null)
                    ProcessNonStringHashTableKey(invocationContext, e.Key);
                // find a match for the key
                HashtableEntryDefinition def = _paramDef.MatchEntry(currentStringKey, invocationContext);
                if (retVal.Contains(def.KeyName))
                    // duplicate key error
                    ProcessDuplicateHashTableKey(invocationContext, currentStringKey, def.KeyName);
                // now the key is verified, need to check the type
                bool matchType = false;
                if (def.AllowedTypes == null || def.AllowedTypes.Length == 0)
                    // we match on any type, it will be up to the entry to further check
                    matchType = true;
                    for (int t = 0; t < def.AllowedTypes.Length; t++)
                        if (e.Value == null)
                            ProcessMissingKeyValue(invocationContext, currentStringKey);
                        if (def.AllowedTypes[t].IsAssignableFrom(e.Value.GetType()))
                if (!matchType)
                    // bad type error
                    ProcessIllegalHashTableKeyValue(invocationContext, currentStringKey, e.Value.GetType(), def.AllowedTypes);
                retVal.Add(def.KeyName, e.Value);
        private void VerifyAndNormalizeParameter(MshParameter parameter,
            for (int k = 0; k < _paramDef.hashEntries.Count; k++)
                if (parameter.hash.ContainsKey(_paramDef.hashEntries[k].KeyName))
                    // we have a key, just do some post processing normalization
                    // retrieve the value
                    object val = parameter.hash[_paramDef.hashEntries[k].KeyName];
                    object newVal = _paramDef.hashEntries[k].Verify(val, invocationContext, originalParameterWasHashTable);
                    if (newVal != null)
                        // if a new value is provided, we need to update the hash entry
                        parameter.hash[_paramDef.hashEntries[k].KeyName] = newVal;
                    // we do not have the key, we might want to have a default value
                    object defaultValue = _paramDef.hashEntries[k].ComputeDefaultValue();
                    if (defaultValue != AutomationNull.Value)
                        // we have a default value, add it
                        parameter.hash[_paramDef.hashEntries[k].KeyName] = defaultValue;
                    else if (_paramDef.hashEntries[k].Mandatory)
                        // no default value and mandatory: we cannot proceed
                        ProcessMissingMandatoryKey(invocationContext, _paramDef.hashEntries[k].KeyName);
        private static void ProcessUnknownParameterType(TerminatingErrorContext invocationContext, object actualObject, Type[] allowedTypes)
            string allowedTypesList = CatenateTypeArray(allowedTypes);
            if (actualObject != null)
                msg = StringUtil.Format(FormatAndOut_MshParameter.UnknownParameterTypeError,
                    actualObject.GetType().FullName, allowedTypesList);
                msg = StringUtil.Format(FormatAndOut_MshParameter.NullParameterTypeError,
                    allowedTypesList);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyUnknownType", msg);
        private static void ProcessDuplicateHashTableKey(TerminatingErrorContext invocationContext, string duplicateKey, string existingKey)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.DuplicateKeyError,
                           duplicateKey, existingKey);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyDuplicate", msg);
        private static void ProcessNullHashTableKey(TerminatingErrorContext invocationContext)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.DictionaryKeyNullError);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyNull", msg);
        private static void ProcessNonStringHashTableKey(TerminatingErrorContext invocationContext, object key)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.DictionaryKeyNonStringError, key.GetType().Name);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyNonString", msg);
        private static void ProcessIllegalHashTableKeyValue(TerminatingErrorContext invocationContext, string key, Type actualType, Type[] allowedTypes)
            if (allowedTypes.Length > 1)
                string legalTypes = CatenateTypeArray(allowedTypes);
                msg = StringUtil.Format(FormatAndOut_MshParameter.IllegalTypeMultiError,
                    actualType.FullName,
                    legalTypes
                errorID = "DictionaryKeyIllegalValue1";
                msg = StringUtil.Format(FormatAndOut_MshParameter.IllegalTypeSingleError,
                    allowedTypes[0]
                errorID = "DictionaryKeyIllegalValue2";
        private static void ProcessMissingKeyValue(TerminatingErrorContext invocationContext, string keyName)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.MissingKeyValueError, keyName);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyMissingValue", msg);
        private static void ProcessMissingMandatoryKey(TerminatingErrorContext invocationContext, string keyName)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.MissingKeyMandatoryEntryError, keyName);
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "DictionaryKeyMandatoryEntry", msg);
        private static string CatenateTypeArray(Type[] arr)
            string[] strings = new string[arr.Length];
            for (int k = 0; k < arr.Length; k++)
                strings[k] = arr[k].FullName;
            return CatenateStringArray(strings);
        internal static string CatenateStringArray(string[] arr)
                sb.Append(arr[k]);
        private readonly CommandParameterDefinition _paramDef = null;

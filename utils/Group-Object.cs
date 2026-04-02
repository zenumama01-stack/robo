    /// PSTuple is a helper class used to create Tuple from an input array.
    internal static class PSTuple
        /// ArrayToTuple is a helper method used to create a tuple for the supplied input array.
        /// <typeparam name="T">The first generic type parameter.</typeparam>
        /// <param name="inputObjects">Input objects used to create a tuple.</param>
        /// <returns>Tuple object.</returns>
        internal static object ArrayToTuple<T>(IList<T> inputObjects)
            return ArrayToTuple(inputObjects, 0);
        /// <param name="startIndex">Start index of the array from which the objects have to considered for the tuple creation.</param>
        private static object ArrayToTuple<T>(IList<T> inputObjects, int startIndex)
            Diagnostics.Assert(inputObjects != null, "inputObjects is null");
            Diagnostics.Assert(inputObjects.Count > 0, "inputObjects is empty");
            switch (inputObjects.Count - startIndex)
                    return Tuple.Create(inputObjects[startIndex]);
                    return Tuple.Create(inputObjects[startIndex], inputObjects[startIndex + 1]);
                    return Tuple.Create(inputObjects[startIndex], inputObjects[startIndex + 1], inputObjects[startIndex + 2]);
                    return Tuple.Create(inputObjects[startIndex], inputObjects[startIndex + 1], inputObjects[startIndex + 2], inputObjects[startIndex + 3]);
                    return Tuple.Create(
                        inputObjects[startIndex],
                        inputObjects[startIndex + 1],
                        inputObjects[startIndex + 2],
                        inputObjects[startIndex + 3],
                        inputObjects[startIndex + 4]);
                        inputObjects[startIndex + 4],
                        inputObjects[startIndex + 5]);
                case 7:
                        inputObjects[startIndex + 5],
                        inputObjects[startIndex + 6]);
                        inputObjects[startIndex + 6],
                        inputObjects[startIndex + 7]);
                        ArrayToTuple(inputObjects, startIndex + 7));
    /// Emitted by Group-Object when the NoElement option is true.
    public sealed class GroupInfoNoElement : GroupInfo
        internal GroupInfoNoElement(OrderByPropertyEntry groupValue) : base(groupValue)
        internal override void Add(PSObject groupValue)
            Count++;
    /// Emitted by Group-Object.
    [DebuggerDisplay("{Name} ({Count})")]
    public class GroupInfo
        internal GroupInfo(OrderByPropertyEntry groupValue)
            Group = new Collection<PSObject>();
            this.Add(groupValue.inputObject);
            GroupValue = groupValue;
            Name = BuildName(groupValue.orderValues);
        internal virtual void Add(PSObject groupValue)
            Group.Add(groupValue);
        private static string BuildName(List<ObjectCommandPropertyValue> propValues)
            foreach (ObjectCommandPropertyValue propValue in propValues)
                var propValuePropertyValue = propValue?.PropertyValue;
                if (propValuePropertyValue != null)
                    if (propValuePropertyValue is ICollection propertyValueItems)
                        sb.Append('{');
                        var length = sb.Length;
                        foreach (object item in propertyValueItems)
                            sb.Append(CultureInfo.CurrentCulture, $"{item}, ");
                        sb = sb.Length > length ? sb.Remove(sb.Length - 2, 2) : sb;
                        sb.Append("}, ");
                        sb.Append(CultureInfo.CurrentCulture, $"{propValuePropertyValue}, ");
            return sb.Length >= 2 ? sb.Remove(sb.Length - 2, 2).ToString() : string.Empty;
        /// Gets the values of the group.
        public ArrayList Values
                ArrayList values = new();
                foreach (ObjectCommandPropertyValue propValue in GroupValue.orderValues)
                    values.Add(propValue.PropertyValue);
                return values;
        /// Gets the number of objects in the group.
        public int Count { get; internal set; }
        /// Gets the list of objects in this group.
        public Collection<PSObject> Group { get; }
        /// Gets the name of the group.
        /// Gets the OrderByPropertyEntry used to build this group object.
        internal OrderByPropertyEntry GroupValue { get; }
    /// Group-Object implementation.
    [Cmdlet(VerbsData.Group, "Object", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096619", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(Hashtable), typeof(GroupInfo))]
    public class GroupObjectCommand : ObjectBase
        #region tracer
        /// An instance of the PSTraceSource class used for trace output.
        [TraceSource("GroupObjectCommand", "Class that has group base implementation")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("GroupObjectCommand", "Class that has group base implementation");
        #endregion tracer
        #region Command Line Switches
        /// Gets or sets the NoElement parameter indicating of the groups should be flattened.
        public SwitchParameter NoElement { get; set; }
        /// Gets or sets the AsHashTable parameter.
        [Parameter(ParameterSetName = "HashTable")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HashTable")]
        [Alias("AHT")]
        public SwitchParameter AsHashTable { get; set; }
        /// Gets or sets the AsString parameter.
        public SwitchParameter AsString { get; set; }
        private readonly List<GroupInfo> _groups = new();
        private readonly OrderByProperty _orderByProperty = new();
        private readonly Dictionary<object, GroupInfo> _tupleToGroupInfoMappingDictionary = new();
        private readonly List<OrderByPropertyEntry> _entriesToOrder = new();
        private OrderByPropertyComparer _orderByPropertyComparer;
        private bool _hasProcessedFirstInputObject;
        private bool _hasDifferentValueTypes;
        private Type[] _propertyTypesCandidate;
        #region utils
        /// Utility function called by Group-Object to create Groups.
        /// <param name="currentObjectEntry">Input object that needs to be grouped.</param>
        /// <param name="noElement">True if we are not accumulating objects.</param>
        /// <param name="groups">List containing Groups.</param>
        /// <param name="groupInfoDictionary">Dictionary used to keep track of the groups with hash of the property values being the key.</param>
        /// <param name="orderByPropertyComparer">The Comparer to be used while comparing to check if new group has to be created.</param>
        private static void DoGrouping(
            OrderByPropertyEntry currentObjectEntry,
            bool noElement,
            List<GroupInfo> groups,
            Dictionary<object, GroupInfo> groupInfoDictionary,
            OrderByPropertyComparer orderByPropertyComparer)
            var currentObjectOrderValues = currentObjectEntry.orderValues;
            if (currentObjectOrderValues != null && currentObjectOrderValues.Count > 0)
                object currentTupleObject = PSTuple.ArrayToTuple(currentObjectOrderValues);
                if (groupInfoDictionary.TryGetValue(currentTupleObject, out var currentGroupInfo))
                    // add this inputObject to an existing group
                    currentGroupInfo.Add(currentObjectEntry.inputObject);
                    bool isCurrentItemGrouped = false;
                    for (int groupsIndex = 0; groupsIndex < groups.Count; groupsIndex++)
                        // Check if the current input object can be converted to one of the already known types
                        // by looking up in the type to GroupInfo mapping.
                        if (orderByPropertyComparer.Compare(groups[groupsIndex].GroupValue, currentObjectEntry) == 0)
                            groups[groupsIndex].Add(currentObjectEntry.inputObject);
                            isCurrentItemGrouped = true;
                    if (!isCurrentItemGrouped)
                        // create a new group
                        s_tracer.WriteLine("Create a new group: {0}", currentObjectOrderValues);
                        GroupInfo newObjGrp = noElement ? new GroupInfoNoElement(currentObjectEntry) : new GroupInfo(currentObjectEntry);
                        groups.Add(newObjGrp);
                        groupInfoDictionary.Add(currentTupleObject, newObjGrp);
        private static void DoOrderedGrouping(
                    if (groups.Count > 0)
                        var lastGroup = groups[groups.Count - 1];
                        if (orderByPropertyComparer.Compare(lastGroup.GroupValue, currentObjectEntry) == 0)
                            lastGroup.Add(currentObjectEntry.inputObject);
                        GroupInfo newObjGrp = noElement
                            ? new GroupInfoNoElement(currentObjectEntry)
                            : new GroupInfo(currentObjectEntry);
        private void WriteNonTerminatingError(Exception exception, string resourceIdAndErrorId, ErrorCategory category)
            Exception ex = new(StringUtil.Format(resourceIdAndErrorId), exception);
            WriteError(new ErrorRecord(ex, resourceIdAndErrorId, category, null));
        #endregion utils
        /// Process every input object to group them.
            if (InputObject != null && InputObject != AutomationNull.Value)
                OrderByPropertyEntry currentEntry;
                if (!_hasProcessedFirstInputObject)
                    Property ??= OrderByProperty.GetDefaultKeyPropertySet(InputObject);
                    _orderByProperty.ProcessExpressionParameter(this, Property);
                    if (AsString && !AsHashTable)
                        ArgumentException ex = new(UtilityCommonStrings.GroupObjectWithHashTable);
                        ErrorRecord er = new(ex, "ArgumentException", ErrorCategory.InvalidArgument, AsString);
                    if (AsHashTable && !AsString && (Property != null && (Property.Length > 1 || _orderByProperty.MshParameterList.Count > 1)))
                        ArgumentException ex = new(UtilityCommonStrings.GroupObjectSingleProperty);
                        ErrorRecord er = new(ex, "ArgumentException", ErrorCategory.InvalidArgument, Property);
                    currentEntry = _orderByProperty.CreateOrderByPropertyEntry(this, InputObject, CaseSensitive, _cultureInfo);
                    bool[] ascending = new bool[currentEntry.orderValues.Count];
                    for (int index = 0; index < currentEntry.orderValues.Count; index++)
                        ascending[index] = true;
                    _orderByPropertyComparer = new OrderByPropertyComparer(ascending, _cultureInfo, CaseSensitive);
                    _hasProcessedFirstInputObject = true;
                _entriesToOrder.Add(currentEntry);
                var currentEntryOrderValues = currentEntry.orderValues;
                if (!_hasDifferentValueTypes)
                    UpdateOrderPropertyTypeInfo(currentEntryOrderValues);
        private void UpdateOrderPropertyTypeInfo(List<ObjectCommandPropertyValue> currentEntryOrderValues)
            if (_propertyTypesCandidate == null)
                _propertyTypesCandidate = currentEntryOrderValues.Select(static c => PSObject.Base(c.PropertyValue)?.GetType()).ToArray();
            if (_propertyTypesCandidate.Length != currentEntryOrderValues.Count)
                _hasDifferentValueTypes = true;
            // check all the types we group on.
            // if we find more than one set of types, _hasDifferentValueTypes is set to true,
            // and we are forced to take a slower code path when we group our objects
            for (int i = 0; i < _propertyTypesCandidate.Length; i++)
                var candidateType = _propertyTypesCandidate[i];
                var propertyType = PSObject.Base(currentEntryOrderValues[i].PropertyValue)?.GetType();
                if (propertyType == null)
                    // we ignore properties without values. We can always compare against null.
                // if we haven't gotten a type for a property yet, update it when we do get a value
                if (propertyType != candidateType)
                    if (candidateType == null)
                        _propertyTypesCandidate[i] = propertyType;
        /// Completes the processing of the gathered group objects.
                // using OrderBy to get stable sort.
                // fast path when we only have the same object types to group
                foreach (var entry in _entriesToOrder.Order(_orderByPropertyComparer))
                    DoOrderedGrouping(entry, NoElement, _groups, _tupleToGroupInfoMappingDictionary, _orderByPropertyComparer);
                    if (Stopping)
                foreach (var entry in _entriesToOrder)
                    DoGrouping(entry, NoElement, _groups, _tupleToGroupInfoMappingDictionary, _orderByPropertyComparer);
            s_tracer.WriteLine(_groups.Count);
            if (_groups.Count > 0)
                if (AsHashTable.IsPresent)
                    StringComparer comparer = CaseSensitive.IsPresent
                        ? StringComparer.CurrentCulture
                        : StringComparer.CurrentCultureIgnoreCase;
                    var hashtable = new Hashtable(comparer);
                            foreach (GroupInfo grp in _groups)
                                hashtable.Add(grp.Name, grp.Group);
                                hashtable.Add(PSObject.Base(grp.Values[0]), grp.Group);
                        WriteNonTerminatingError(e, UtilityCommonStrings.InvalidOperation, ErrorCategory.InvalidArgument);
                    WriteObject(hashtable);
                    WriteObject(_groups, true);

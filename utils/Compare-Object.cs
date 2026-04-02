using Microsoft.PowerShell.Commands.Internal.Format;
    [Cmdlet(VerbsData.Compare, "Object", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096605",
        RemotingCapability = RemotingCapability.None)]
    public sealed class CompareObjectCommand : ObjectCmdletBase
        public PSObject[] ReferenceObject { get; set; }
        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true)]
        public PSObject[] DifferenceObject { get; set; }
        public int SyncWindow { get; set; } = int.MaxValue;
        public object[] Property { get; set; }
        /* not implemented
        public SwitchParameter IgnoreWhiteSpace
            get { return _ignoreWhiteSpace; }
            set { _ignoreWhiteSpace = value; }
        private bool _ignoreWhiteSpace = false;
        public SwitchParameter ExcludeDifferent
            get { return _excludeDifferent; }
            set { _excludeDifferent = value; }
        private bool _excludeDifferent /*=false*/;
        public SwitchParameter IncludeEqual
                return _includeEqual;
                _isIncludeEqualSpecified = true;
                _includeEqual = value;
        private bool _includeEqual /* = false */;
        private bool _isIncludeEqualSpecified /* = false */;
        private List<OrderByPropertyEntry> _referenceEntries;
        private readonly List<OrderByPropertyEntry> _referenceEntryBacklog
        private readonly List<OrderByPropertyEntry> _differenceEntryBacklog
        private OrderByProperty _orderByProperty = null;
        private OrderByPropertyComparer _comparer = null;
        private int _referenceObjectIndex /* = 0 */;
        // These are programmatic strings, not subject to INTL
        private const string SideIndicatorPropertyName = "SideIndicator";
        private const string SideIndicatorMatch = "==";
        private const string SideIndicatorReference = "<=";
        private const string SideIndicatorDifference = "=>";
        private const string InputObjectPropertyName = "InputObject";
        /// The following is the matching algorithm:
        /// Retrieve the incoming object (differenceEntry) if any
        /// Retrieve the next reference object (referenceEntry) if any
        /// If differenceEntry matches referenceEntry
        ///   Emit referenceEntry as a match
        ///   Return
        /// If differenceEntry matches any entry in referenceEntryBacklog
        ///   Emit the backlog entry as a match
        ///   Remove the backlog entry from referenceEntryBacklog
        ///   Clear differenceEntry
        /// If referenceEntry (if any) matches any entry in differenceEntryBacklog
        ///   Remove the backlog entry from differenceEntryBacklog
        ///   Clear referenceEntry
        /// If differenceEntry is still present
        ///   If SyncWindow is 0
        ///     Emit differenceEntry as unmatched
        ///   Else
        ///     While there is no space in differenceEntryBacklog
        ///       Emit oldest entry in differenceEntryBacklog as unmatched
        ///       Remove oldest entry from differenceEntryBacklog
        ///     Add differenceEntry to differenceEntryBacklog
        /// If referenceEntry is still present
        ///     Emit referenceEntry as unmatched
        ///     While there is no space in referenceEntryBacklog
        ///       Emit oldest entry in referenceEntryBacklog as unmatched
        ///       Remove oldest entry from referenceEntryBacklog
        ///     Add referenceEntry to referenceEntryBacklog.
        /// <param name="differenceEntry"></param>
        private void Process(OrderByPropertyEntry differenceEntry)
            Diagnostics.Assert(_referenceEntries != null, "null referenceEntries");
            // Retrieve the next reference object (referenceEntry) if any
            OrderByPropertyEntry referenceEntry = null;
            if (_referenceObjectIndex < _referenceEntries.Count)
                referenceEntry = _referenceEntries[_referenceObjectIndex++];
            // If differenceEntry matches referenceEntry
            //   Emit referenceEntry as a match
            //   Return
            // 2005/07/19 Switched order of referenceEntry and differenceEntry
            //   so that we cast differenceEntry to the type of referenceEntry.
            if (referenceEntry != null && differenceEntry != null &&
                _comparer.Compare(referenceEntry, differenceEntry) == 0)
                EmitMatch(referenceEntry);
            // If differenceEntry matches any entry in referenceEntryBacklog
            //   Emit the backlog entry as a match
            //   Remove the backlog entry from referenceEntryBacklog
            //   Clear differenceEntry
            OrderByPropertyEntry matchingEntry =
                MatchAndRemove(differenceEntry, _referenceEntryBacklog);
            if (matchingEntry != null)
                EmitMatch(matchingEntry);
                differenceEntry = null;
            // If referenceEntry (if any) matches any entry in differenceEntryBacklog
            //   Remove the backlog entry from differenceEntryBacklog
            //   Clear referenceEntry
            matchingEntry =
                MatchAndRemove(referenceEntry, _differenceEntryBacklog);
                referenceEntry = null;
            // If differenceEntry is still present
            //   If SyncWindow is 0
            //     Emit differenceEntry as unmatched
            //   Else
            //     While there is no space in differenceEntryBacklog
            //       Emit oldest entry in differenceEntryBacklog as unmatched
            //       Remove oldest entry from differenceEntryBacklog
            //     Add differenceEntry to differenceEntryBacklog
            if (differenceEntry != null)
                if (SyncWindow > 0)
                    while (_differenceEntryBacklog.Count >= SyncWindow)
                        EmitDifferenceOnly(_differenceEntryBacklog[0]);
                        _differenceEntryBacklog.RemoveAt(0);
                    _differenceEntryBacklog.Add(differenceEntry);
                    EmitDifferenceOnly(differenceEntry);
            // If referenceEntry is still present
            //     Emit referenceEntry as unmatched
            //     While there is no space in referenceEntryBacklog
            //       Emit oldest entry in referenceEntryBacklog as unmatched
            //       Remove oldest entry from referenceEntryBacklog
            //     Add referenceEntry to referenceEntryBacklog
            if (referenceEntry != null)
                    while (_referenceEntryBacklog.Count >= SyncWindow)
                        EmitReferenceOnly(_referenceEntryBacklog[0]);
                        _referenceEntryBacklog.RemoveAt(0);
                    _referenceEntryBacklog.Add(referenceEntry);
                    EmitReferenceOnly(referenceEntry);
        private void InitComparer()
            if (_comparer != null)
            List<PSObject> referenceObjectList = new(ReferenceObject);
            _orderByProperty = new OrderByProperty(
                this, referenceObjectList, Property, true, _cultureInfo, CaseSensitive);
            Diagnostics.Assert(_orderByProperty.Comparer != null, "no comparer");
                _orderByProperty.OrderMatrix != null &&
                _orderByProperty.OrderMatrix.Count == ReferenceObject.Length,
                "no OrderMatrix");
            if (_orderByProperty.Comparer == null || _orderByProperty.OrderMatrix == null || _orderByProperty.OrderMatrix.Count == 0)
            _comparer = _orderByProperty.Comparer;
            _referenceEntries = _orderByProperty.OrderMatrix;
        private OrderByPropertyEntry MatchAndRemove(
            OrderByPropertyEntry match,
            List<OrderByPropertyEntry> list)
            if (match == null || list == null)
            Diagnostics.Assert(_comparer != null, "null comparer");
            for (int i = 0; i < list.Count; i++)
                OrderByPropertyEntry listEntry = list[i];
                Diagnostics.Assert(listEntry != null, "null listEntry " + i);
                if (_comparer.Compare(match, listEntry) == 0)
                    list.RemoveAt(i);
                    return listEntry;
        #region Emit
        private void EmitMatch(OrderByPropertyEntry entry)
            if (_includeEqual)
                Emit(entry, SideIndicatorMatch);
        private void EmitDifferenceOnly(OrderByPropertyEntry entry)
            if (!ExcludeDifferent)
                Emit(entry, SideIndicatorDifference);
        private void EmitReferenceOnly(OrderByPropertyEntry entry)
                Emit(entry, SideIndicatorReference);
        private void Emit(OrderByPropertyEntry entry, string sideIndicator)
            Diagnostics.Assert(entry != null, "null entry");
            PSObject mshobj;
                mshobj = PSObject.AsPSObject(entry.inputObject);
                mshobj = new PSObject();
                if (Property == null || Property.Length == 0)
                    PSNoteProperty inputNote = new(
                        InputObjectPropertyName, entry.inputObject);
                    mshobj.Properties.Add(inputNote);
                    List<MshParameter> mshParameterList = _orderByProperty.MshParameterList;
                    Diagnostics.Assert(mshParameterList != null, "null mshParameterList");
                    Diagnostics.Assert(mshParameterList.Count == Property.Length, "mshParameterList.Count " + mshParameterList.Count);
                    for (int i = 0; i < Property.Length; i++)
                        // 2005/07/05 This is the closest we can come to
                        // the string typed by the user
                        MshParameter mshParameter = mshParameterList[i];
                        Diagnostics.Assert(mshParameter != null, "null mshParameter");
                        Hashtable hash = mshParameter.hash;
                        Diagnostics.Assert(hash != null, "null hash");
                        object prop = hash[FormatParameterDefinitionKeys.ExpressionEntryKey];
                        Diagnostics.Assert(prop != null, "null prop");
                        string propName = prop.ToString();
                        PSNoteProperty propertyNote = new(
                            propName,
                            entry.orderValues[i].PropertyValue);
                            mshobj.Properties.Add(propertyNote);
                            // this is probably a duplicate add
            mshobj.Properties.Remove(SideIndicatorPropertyName);
            PSNoteProperty sideNote = new(
                SideIndicatorPropertyName, sideIndicator);
            mshobj.Properties.Add(sideNote);
            WriteObject(mshobj);
        #endregion Emit
        /// If the parameter 'ExcludeDifferent' is present, then the 'IncludeEqual'
        /// switch is turned on unless it's turned off by the user specifically.
            if (ExcludeDifferent)
                if (_isIncludeEqualSpecified && !_includeEqual)
                _includeEqual = true;
            if (ReferenceObject == null || ReferenceObject.Length == 0)
                HandleDifferenceObjectOnly();
            else if (DifferenceObject == null || DifferenceObject.Length == 0)
                HandleReferenceObjectOnly();
            if (_comparer == null && DifferenceObject.Length > 0)
                InitComparer();
            List<PSObject> differenceList = new(DifferenceObject);
            List<OrderByPropertyEntry> differenceEntries =
                OrderByProperty.CreateOrderMatrix(
                this, differenceList, _orderByProperty.MshParameterList);
            foreach (OrderByPropertyEntry incomingEntry in differenceEntries)
                Process(incomingEntry);
            // Clear remaining reference objects if there are more
            // reference objects than difference objects
            if (_referenceEntries != null)
                while (_referenceObjectIndex < _referenceEntries.Count)
                    Process(null);
            // emit all remaining backlogged objects
            foreach (OrderByPropertyEntry differenceEntry in _differenceEntryBacklog)
            _differenceEntryBacklog.Clear();
            foreach (OrderByPropertyEntry referenceEntry in _referenceEntryBacklog)
            _referenceEntryBacklog.Clear();
        private void HandleDifferenceObjectOnly()
            if (DifferenceObject == null || DifferenceObject.Length == 0)
                this, differenceList, Property, true, _cultureInfo, CaseSensitive);
            foreach (OrderByPropertyEntry entry in differenceEntries)
                EmitDifferenceOnly(entry);
        private void HandleReferenceObjectOnly()

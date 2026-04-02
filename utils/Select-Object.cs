    internal sealed class SelectObjectExpressionParameterDefinition : CommandParameterDefinition
            this.hashEntries.Add(new NameEntryDefinition());
    [Cmdlet(VerbsCommon.Select, "Object", DefaultParameterSetName = "DefaultParameter",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096716", RemotingCapability = RemotingCapability.None)]
    public sealed class SelectObjectCommand : PSCmdlet
        [Parameter(Position = 0, ParameterSetName = "DefaultParameter")]
        [Parameter(Position = 0, ParameterSetName = "SkipLastParameter")]
        [Parameter(ParameterSetName = "DefaultParameter")]
        [Parameter(ParameterSetName = "SkipLastParameter")]
        public string[] ExcludeProperty { get; set; }
        public string ExpandProperty { get; set; }
        public SwitchParameter Unique
            get { return _unique; }
            set { _unique = value; }
        private bool _unique;
        /// Used in combination with Unique switch parameter.
        // NTRAID#Windows Out Of Band Releases-927878-2006/03/02
        // Allow zero
        public int Last
            get { return _last; }
            set { _last = value; _firstOrLastSpecified = true; }
        private int _last = 0;
        public int First
            get { return _first; }
            set { _first = value; _firstOrLastSpecified = true; }
        private int _first = 0;
        private bool _firstOrLastSpecified;
        /// Skips the specified number of items from top when used with First, from end when used with Last or SkipLast.
        public int Skip { get; set; }
        /// Skip the specified number of items from end.
        public int SkipLast { get; set; }
        /// With this switch present, the cmdlet won't "short-circuit"
        /// (i.e. won't stop upstream cmdlets after it knows that no further objects will be emitted downstream).
        [Parameter(ParameterSetName = "IndexParameter")]
        /// Used to display the object at the specified index.
                return _index;
                _index = value;
                _indexSpecified = true;
                _isIncludeIndex = true;
                Array.Sort(_index);
        /// Used to display all objects at the specified indices.
        [Parameter(ParameterSetName = "SkipIndexParameter")]
        public int[] SkipIndex
                _isIncludeIndex = false;
        private int[] _index;
        private bool _indexSpecified;
        private bool _isIncludeIndex;
        private SelectObjectQueue _selectObjectQueue;
        private sealed class SelectObjectQueue : Queue<PSObject>
            internal SelectObjectQueue(int first, int last, int skip, int skipLast, bool firstOrLastSpecified)
                _first = first;
                _last = last;
                _skip = skip;
                _skipLast = skipLast;
                _firstOrLastSpecified = firstOrLastSpecified;
            public bool AllRequestedObjectsProcessed
                    return _firstOrLastSpecified && _last == 0 && _first != 0 && _streamedObjectCount >= _first;
            public new void Enqueue(PSObject obj)
                if (_last > 0 && this.Count >= (_last + _skip) && _first == 0)
                    base.Dequeue();
                else if (_last > 0 && this.Count >= _last && _first != 0)
                base.Enqueue(obj);
            public PSObject StreamingDequeue()
                // if skip parameter is not mentioned or there are no more objects to skip
                if (_skip == 0)
                    if (_skipLast > 0)
                        // We are going to skip some items from end, but it's okay to process
                        // the early input objects once we have more items in queue than the
                        // specified 'skipLast' value.
                        if (this.Count > _skipLast)
                            return Dequeue();
                        if (_streamedObjectCount < _first || !_firstOrLastSpecified)
                            Diagnostics.Assert(this.Count > 0, "Streaming an empty queue");
                            _streamedObjectCount++;
                        if (_last == 0)
                            Dequeue();
                    // if last parameter is not mentioned,remove the objects and decrement the skip
                        _skip--;
                    else if (_first != 0)
            private int _streamedObjectCount;
            private readonly int _first;
            private readonly int _last;
            private int _skip;
            private readonly int _skipLast;
            private readonly bool _firstOrLastSpecified;
        /// List of processed parameters obtained from the Expression array.
        /// Singleton list of process parameters obtained from ExpandProperty.
        private List<MshParameter> _expandMshParameterList;
        private PSPropertyExpressionFilter _exclusionFilter;
        private sealed class UniquePSObjectHelper
            internal UniquePSObjectHelper(PSObject o, int notePropertyCount)
                WrittenObject = o;
                NotePropertyCount = notePropertyCount;
            internal readonly PSObject WrittenObject;
            internal int NotePropertyCount { get; }
        private List<UniquePSObjectHelper> _uniques = null;
        private void ProcessExpressionParameter()
                new(new SelectObjectExpressionParameterDefinition());
            if ((Property != null) && (Property.Length != 0))
                // Build property list taking into account the wildcards and @{name=;expression=}
                _propertyMshParameterList = processor.ProcessParameters(Property, invocationContext);
                // Property don't exist
                _propertyMshParameterList = new List<MshParameter>();
            if (!string.IsNullOrEmpty(ExpandProperty))
                _expandMshParameterList = processor.ProcessParameters(new string[] { ExpandProperty }, invocationContext);
            if (ExcludeProperty != null)
                _exclusionFilter = new PSPropertyExpressionFilter(ExcludeProperty);
                // ExcludeProperty implies -Property * for better UX
                if ((Property == null) || (Property.Length == 0))
                    Property = new object[] { "*" };
        private void ProcessObject(PSObject inputObject)
            if ((Property == null || Property.Length == 0) && string.IsNullOrEmpty(ExpandProperty))
                FilteredWriteObject(inputObject, new List<PSNoteProperty>());
            // If property parameter is mentioned
            List<PSNoteProperty> matchedProperties = new();
                ProcessParameter(p, inputObject, matchedProperties);
            if (string.IsNullOrEmpty(ExpandProperty))
                PSObject result = new();
                if (matchedProperties.Count != 0)
                    HashSet<string> propertyNames = new(StringComparer.OrdinalIgnoreCase);
                    foreach (PSNoteProperty noteProperty in matchedProperties)
                            if (!propertyNames.Contains(noteProperty.Name))
                                propertyNames.Add(noteProperty.Name);
                                result.Properties.Add(noteProperty);
                                WriteAlreadyExistingPropertyError(noteProperty.Name, inputObject,
                                    "AlreadyExistingUserSpecifiedPropertyNoExpand");
                FilteredWriteObject(result, matchedProperties);
                ProcessExpandParameter(_expandMshParameterList[0], inputObject, matchedProperties);
        private void ProcessParameter(MshParameter p, PSObject inputObject, List<PSNoteProperty> result)
            string name = p.GetEntry(NameEntryDefinition.NameEntryKey) as string;
            List<PSPropertyExpressionResult> expressionResults = new();
            foreach (PSPropertyExpression resolvedName in ex.ResolveNames(inputObject))
                if (_exclusionFilter == null || !_exclusionFilter.IsMatch(resolvedName))
                    List<PSPropertyExpressionResult> tempExprResults = resolvedName.GetValues(inputObject);
                    if (tempExprResults == null)
                    foreach (PSPropertyExpressionResult mshExpRes in tempExprResults)
                        expressionResults.Add(mshExpRes);
            // allow 'Select-Object -Property noexist-name' to return a PSObject with property noexist-name,
            // unless noexist-name itself contains wildcards
            if (expressionResults.Count == 0 && !ex.HasWildCardCharacters)
                expressionResults.Add(new PSPropertyExpressionResult(null, ex, null));
            // if we have an expansion, renaming is not acceptable
            else if (!string.IsNullOrEmpty(name) && expressionResults.Count > 1)
                string errorMsg = SelectObjectStrings.RenamingMultipleResults;
                    new InvalidOperationException(errorMsg),
                    "RenamingMultipleResults",
                // filter the exclusions, if any
                if (_exclusionFilter != null && _exclusionFilter.IsMatch(r.ResolvedExpression))
                PSNoteProperty mshProp;
                    string resolvedExpressionName = r.ResolvedExpression.ToString();
                    if (string.IsNullOrEmpty(resolvedExpressionName))
                                                        "Property",
                                                        SelectObjectStrings.EmptyScriptBlockAndNoName);
                            "EmptyScriptBlockAndNoName",
                            ErrorCategory.InvalidArgument, null));
                    mshProp = new PSNoteProperty(resolvedExpressionName, r.Result);
                    mshProp = new PSNoteProperty(name, r.Result);
                result.Add(mshProp);
        private void ProcessExpandParameter(MshParameter p, PSObject inputObject,
            List<PSNoteProperty> matchedProperties)
            List<PSPropertyExpressionResult> expressionResults = ex.GetValues(inputObject);
                    PSTraceSource.NewArgumentException("ExpandProperty", SelectObjectStrings.PropertyNotFound, ExpandProperty),
                    "ExpandPropertyNotFound",
                throw new SelectObjectException(errorRecord);
            if (expressionResults.Count > 1)
                    PSTraceSource.NewArgumentException("ExpandProperty", SelectObjectStrings.MutlipleExpandProperties, ExpandProperty),
                    "MutlipleExpandProperties",
            PSPropertyExpressionResult r = expressionResults[0];
                // ignore the property value if it's null
                if (r.Result == null)
                System.Collections.IEnumerable results = LanguagePrimitives.GetEnumerable(r.Result);
                if (results == null)
                    // add NoteProperties if there is any
                    // If r.Result is a base object, we don't want to associate the NoteProperty
                    // directly with it. We want the NoteProperty to be associated only with this
                    // particular PSObject, so that when the user uses the base object else where,
                    // its members remain the same as before the Select-Object command run.
                    PSObject expandedObject = PSObject.AsPSObject(r.Result, true);
                    AddNoteProperties(expandedObject, inputObject, matchedProperties);
                    FilteredWriteObject(expandedObject, matchedProperties);
                foreach (object expandedValue in results)
                    // ignore the element if it's null
                    if (expandedValue == null)
                    // If expandedValue is a base object, we don't want to associate the NoteProperty
                    PSObject expandedObject = PSObject.AsPSObject(expandedValue, true);
                    "PropertyEvaluationExpand",
        private void AddNoteProperties(PSObject expandedObject, PSObject inputObject, IEnumerable<PSNoteProperty> matchedProperties)
                    if (expandedObject.Properties[noteProperty.Name] != null)
                        WriteAlreadyExistingPropertyError(noteProperty.Name, inputObject, "AlreadyExistingUserSpecifiedPropertyExpand");
                        expandedObject.Properties.Add(noteProperty);
        private void WriteAlreadyExistingPropertyError(string name, object inputObject, string errorId)
                PSTraceSource.NewArgumentException("Property", SelectObjectStrings.AlreadyExistingProperty, name),
        private void FilteredWriteObject(PSObject obj, List<PSNoteProperty> addedNoteProperties)
            Diagnostics.Assert(obj != null, "This command should never write null");
            if (!_unique)
                if (obj != AutomationNull.Value)
                    SetPSCustomObject(obj, newPSObject: addedNoteProperties.Count > 0);
            // if only unique is mentioned
            else if ((_unique))
                bool isObjUnique = true;
                foreach (UniquePSObjectHelper uniqueObj in _uniques)
                    ObjectCommandComparer comparer = new(
                    if ((comparer.Compare(obj.BaseObject, uniqueObj.WrittenObject.BaseObject) == 0) &&
                        (uniqueObj.NotePropertyCount == addedNoteProperties.Count))
                        bool found = true;
                        foreach (PSNoteProperty note in addedNoteProperties)
                            PSMemberInfo prop = uniqueObj.WrittenObject.Properties[note.Name];
                            if (prop == null || comparer.Compare(prop.Value, note.Value) != 0)
                            isObjUnique = false;
                if (isObjUnique)
                    _uniques.Add(new UniquePSObjectHelper(obj, addedNoteProperties.Count));
        private void SetPSCustomObject(PSObject psObj, bool newPSObject)
            if (psObj.ImmediateBaseObject is PSCustomObject)
                var typeName = "Selected." + InputObject.BaseObject.GetType().ToString();
                if (newPSObject || !psObj.TypeNames.Contains(typeName))
                    psObj.TypeNames.Insert(0, typeName);
        private void ProcessObjectAndHandleErrors(PSObject pso)
            Diagnostics.Assert(pso != null, "Caller should verify pso != null");
                ProcessObject(pso);
            catch (SelectObjectException e)
                WriteError(e.ErrorRecord);
            ProcessExpressionParameter();
            if (_unique)
                _uniques = new List<UniquePSObjectHelper>();
            _selectObjectQueue = new SelectObjectQueue(_first, _last, Skip, SkipLast, _firstOrLastSpecified);
        /// Handles processing of InputObject.
            if (InputObject != AutomationNull.Value && InputObject != null)
                if (_indexSpecified)
                    ProcessIndexed();
                    _selectObjectQueue.Enqueue(InputObject);
                    PSObject streamingInputObject = _selectObjectQueue.StreamingDequeue();
                    if (streamingInputObject != null)
                        ProcessObjectAndHandleErrors(streamingInputObject);
                    if (_selectObjectQueue.AllRequestedObjectsProcessed && !this.Wait)
                        this.EndProcessing();
                        throw new StopUpstreamCommandsException(this);
        /// The index of the active index filter.
        private int _currentFilterIndex;
        /// The index of the object being processed.
        private int _currentObjectIndex;
        /// Handles processing of InputObject if -Index or -SkipIndex is specified.
        private void ProcessIndexed()
            if (_isIncludeIndex)
                if (_currentFilterIndex < _index.Length)
                    int nextIndexToOutput = _index[_currentFilterIndex];
                    if (_currentObjectIndex == nextIndexToOutput)
                        ProcessObjectAndHandleErrors(InputObject);
                        while ((_currentFilterIndex < _index.Length) && (_index[_currentFilterIndex] == nextIndexToOutput))
                            _currentFilterIndex++;
                if (!Wait && _currentFilterIndex >= _index.Length)
                    EndProcessing();
                _currentObjectIndex++;
                    int nextIndexToSkip = _index[_currentFilterIndex];
                    if (_currentObjectIndex != nextIndexToSkip)
                        while ((_currentFilterIndex < _index.Length) && (_index[_currentFilterIndex] == nextIndexToSkip))
        /// Completes the processing of Input.
            // We can skip this part for 'IndexParameter' and 'SkipLastParameter' sets because:
            //   1. 'IndexParameter' set doesn't use selectObjectQueue.
            //   2. 'SkipLastParameter' set should have processed all valid input in the ProcessRecord.
            if (ParameterSetName == "DefaultParameter")
                if (_first != 0)
                    while ((_selectObjectQueue.Count > 0))
                        ProcessObjectAndHandleErrors(_selectObjectQueue.Dequeue());
                        int lenQueue = _selectObjectQueue.Count;
                        if (lenQueue > Skip)
            if (_uniques != null)
                foreach (UniquePSObjectHelper obj in _uniques)
                    if (obj.WrittenObject == null || obj.WrittenObject == AutomationNull.Value)
                    WriteObject(obj.WrittenObject);
    /// Used only internally for select-object.
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This exception is internal and never thrown by any public API")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This exception is internal and never thrown by any public API")]
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "This exception is internal and never thrown by any public API")]
    internal sealed class SelectObjectException : SystemException
        internal ErrorRecord ErrorRecord { get; }
        internal SelectObjectException(ErrorRecord errorRecord)
            ErrorRecord = errorRecord;

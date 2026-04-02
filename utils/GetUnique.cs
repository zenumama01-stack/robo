    [Cmdlet(VerbsCommon.Get, "Unique", DefaultParameterSetName = "AsString",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097028", RemotingCapability = RemotingCapability.None)]
    public sealed class GetUniqueCommand : PSCmdlet
        public PSObject InputObject { get; set; } = AutomationNull.Value;
        /// This parameter specifies that objects should be converted to
        /// strings and the strings should be compared.
        [Parameter(ParameterSetName = "AsString")]
            get { return _asString; }
            set { _asString = value; }
        private bool _asString;
        /// This parameter specifies that just the types of the objects
        /// should be compared.
        [Parameter(ParameterSetName = "UniqueByType")]
        public SwitchParameter OnType
            get { return _onType; }
            set { _onType = value; }
        private bool _onType = false;
        /// Gets or sets case insensitive switch for string comparison.
        public SwitchParameter CaseInsensitive { get; set; }
            bool isUnique = true;
            if (_lastObject == null)
                // always write first object, but return nothing
                // on "MSH> get-unique"
                if (AutomationNull.Value == InputObject)
            else if (OnType)
                isUnique = (InputObject.InternalTypeNames[0] != _lastObject.InternalTypeNames[0]);
            else if (AsString)
                string inputString = InputObject.ToString();
                _lastObjectAsString ??= _lastObject.ToString();
                if (string.Equals(
                    inputString,
                    _lastObjectAsString,
                    CaseInsensitive.IsPresent ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
                    isUnique = false;
                    _lastObjectAsString = inputString;
            else // compare as objects
                _comparer ??= new ObjectCommandComparer(
                    ascending: true,
                    caseSensitive: !CaseInsensitive.IsPresent);
                isUnique = (_comparer.Compare(InputObject, _lastObject) != 0);
            if (isUnique)
                _lastObject = InputObject;
        private PSObject _lastObject = null;
        private string _lastObjectAsString = null;
        private ObjectCommandComparer _comparer = null;

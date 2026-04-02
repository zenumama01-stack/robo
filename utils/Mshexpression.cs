    /// Class that represents the results from evaluating a PSPropertyExpression against an object.
    public class PSPropertyExpressionResult
        /// Create a property expression result containing the original object, matching property expression
        /// and any exception generated during the match process.
        public PSPropertyExpressionResult(object res, PSPropertyExpression re, Exception e)
            Result = res;
            ResolvedExpression = re;
            Exception = e;
        /// The value of the object property matched by this property expression.
        public object Result { get; } = null;
        /// The original property expression fully resolved.
        public PSPropertyExpression ResolvedExpression { get; } = null;
        /// Any exception thrown while evaluating the expression.
        public Exception Exception { get; } = null;
    /// PSPropertyExpression class. This class is used to get the names and/or values of properties
    /// on an object. A property expression can be constructed using either a wildcard expression string
    /// or a scriptblock to use to get the property value.
    public class PSPropertyExpression
        /// <param name="s">Expression.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PSPropertyExpression(string s)
            : this(s, false)
        /// Create a property expression with a wildcard pattern.
        /// <param name="s">Property name pattern to match.</param>
        /// <param name="isResolved"><see langword="true"/> if no further attempts should be made to resolve wildcards.</param>
        public PSPropertyExpression(string s, bool isResolved)
                throw PSTraceSource.NewArgumentNullException(nameof(s));
            _stringValue = s;
            _isResolved = isResolved;
        /// Create a property expression with a ScriptBlock.
        /// <param name="scriptBlock">ScriptBlock to evaluate when retrieving the property value from an object.</param>
        public PSPropertyExpression(ScriptBlock scriptBlock)
            if (scriptBlock == null)
                throw PSTraceSource.NewArgumentNullException(nameof(scriptBlock));
            Script = scriptBlock;
        /// The ScriptBlock for this expression to use when matching.
        public ScriptBlock Script { get; } = null;
        /// ToString() implementation for the property expression.
                return Script.ToString();
            return _stringValue;
        /// Resolve the names matched by the expression.
        /// <param name="target">The object to apply the expression against.</param>
        public List<PSPropertyExpression> ResolveNames(PSObject target)
            return ResolveNames(target, true);
        /// Indicates if the pattern has wildcard characters in it. If the supplied pattern was
        /// a scriptblock, this will be false.
        public bool HasWildCardCharacters
                return WildcardPattern.ContainsWildcardCharacters(_stringValue);
        /// <param name="expand">If the matched properties are property sets, expand them.</param>
        public List<PSPropertyExpression> ResolveNames(PSObject target, bool expand)
            if (_isResolved)
                retVal.Add(this);
                // script block, just add it to the list and be done
                PSPropertyExpression ex = new PSPropertyExpression(Script);
                ex._isResolved = true;
                retVal.Add(ex);
            // If the object passed in is a hashtable, then turn it into a PSCustomObject so
            // that property expressions can work on it.
            var wrappedTarget = IfHashtableWrapAsPSCustomObject(target, out bool wasHashtable);
            // we have a string value
            IEnumerable<PSMemberInfo> members;
            if (HasWildCardCharacters)
                // get the members first: this will expand the globbing on each parameter
                members = wrappedTarget.Members.Match(
                    _stringValue,
                    PSMemberTypes.Properties | PSMemberTypes.PropertySet | PSMemberTypes.Dynamic);
                // if target was a hashtable and no result is found from the keys, then use property value if available
                if (wasHashtable && !members.Any())
                    members = target.Members.Match(
                // we have no globbing: try an exact match, because this is quicker.
                PSMemberInfo x = wrappedTarget.Members[_stringValue];
                    if (wasHashtable)
                        x = target.Members[_stringValue];
                    else if (wrappedTarget.BaseObject is System.Dynamic.IDynamicMetaObjectProvider)
                        // We could check if GetDynamicMemberNames includes the name...  but
                        // GetDynamicMemberNames is only a hint, not a contract, so we'd want
                        // to attempt the binding whether it's in there or not.
                        x = new PSDynamicMember(_stringValue);
                List<PSMemberInfo> temp = new List<PSMemberInfo>();
                if (x != null)
                    temp.Add(x);
                members = temp;
            // we now have a list of members, we have to expand property sets
            // and remove duplicates
            List<PSMemberInfo> temporaryMemberList = new List<PSMemberInfo>();
            foreach (PSMemberInfo member in members)
                // it can be a property set
                if (member is PSPropertySet propertySet)
                    if (expand)
                        // NOTE: we expand the property set under the
                        // assumption that it contains property names that
                        // do not require any further expansion
                        Collection<string> references = propertySet.ReferencedPropertyNames;
                        for (int j = 0; j < references.Count; j++)
                            ReadOnlyPSMemberInfoCollection<PSPropertyInfo> propertyMembers =
                                                target.Properties.Match(references[j]);
                            for (int jj = 0; jj < propertyMembers.Count; jj++)
                                temporaryMemberList.Add(propertyMembers[jj]);
                // it can be a property
                else if (member is PSPropertyInfo)
                    temporaryMemberList.Add(member);
                // it can be a dynamic member
                else if (member is PSDynamicMember)
            var allMembers = new HashSet<string>();
            // build the list of unique values: remove the possible duplicates
            // from property set expansion
            foreach (PSMemberInfo m in temporaryMemberList)
                if (!allMembers.Contains(m.Name))
                    PSPropertyExpression ex = new PSPropertyExpression(m.Name);
                    allMembers.Add(m.Name);
        /// Gets the values of the object properties matched by this expression.
        /// <param name="target">The object to match against.</param>
        public List<PSPropertyExpressionResult> GetValues(PSObject target)
            return GetValues(target, true, true);
        /// <param name="expand">If the matched properties are parameter sets, expand them.</param>
        /// <param name="eatExceptions">If true, any exceptions that occur during the match process are ignored.</param>
        public List<PSPropertyExpressionResult> GetValues(PSObject target, bool expand, bool eatExceptions)
            List<PSPropertyExpressionResult> retVal = new List<PSPropertyExpressionResult>();
            // process the script case
                PSPropertyExpression scriptExpression = new PSPropertyExpression(Script);
                PSPropertyExpressionResult r = scriptExpression.GetValue(target, eatExceptions);
                retVal.Add(r);
            foreach (PSPropertyExpression resolvedName in ResolveNames(target, expand))
                PSPropertyExpressionResult result = resolvedName.GetValue(target, eatExceptions);
                retVal.Add(result);
        private CallSite<Func<CallSite, object, object>> _getValueDynamicSite;
        private PSPropertyExpressionResult GetValue(PSObject target, bool eatExceptions)
                    result = Script.DoInvokeReturnAsIs(
                        errorHandlingBehavior: ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe,
                        dollarUnder: target,
                        input: AutomationNull.Value,
                    _getValueDynamicSite ??=
                        CallSite<Func<CallSite, object, object>>.Create(
                            PSGetMemberBinder.Get(
                                classScope: (Type)null,
                                @static: false));
                    result = _getValueDynamicSite.Target.Invoke(_getValueDynamicSite, target);
                return new PSPropertyExpressionResult(result, this, null);
                if (eatExceptions)
                    return new PSPropertyExpressionResult(null, this, e);
        private static PSObject IfHashtableWrapAsPSCustomObject(PSObject target, out bool wrapped)
            wrapped = false;
            if (PSObject.Base(target) is Hashtable targetAsHash)
                wrapped = true;
                return (PSObject)(LanguagePrimitives.ConvertPSObjectToType(
                    targetAsHash,
                    typeof(PSObject),
                    recursion: false,
                    formatProvider: null,
                    ignoreUnknownMembers: true));
            return target;
        // private members
        private readonly string _stringValue;
        private bool _isResolved = false;
    /// Helper class to do wildcard matching on PSPropertyExpressions.
    internal sealed class PSPropertyExpressionFilter
        /// Initializes a new instance of the <see cref="PSPropertyExpressionFilter"/> class
        /// with the specified array of patterns.
        /// <param name="wildcardPatternsStrings">Array of pattern strings to use.</param>
        internal PSPropertyExpressionFilter(string[] wildcardPatternsStrings)
            ArgumentNullException.ThrowIfNull(wildcardPatternsStrings);
            _wildcardPatterns = new WildcardPattern[wildcardPatternsStrings.Length];
            for (int k = 0; k < wildcardPatternsStrings.Length; k++)
                _wildcardPatterns[k] = WildcardPattern.Get(wildcardPatternsStrings[k], WildcardOptions.IgnoreCase);
        /// Try to match the expression against the array of wildcard patterns.
        /// The first match short-circuits the search.
        /// <param name="expression">PSPropertyExpression to test against.</param>
        /// <returns>True if there is a match, else false.</returns>
        internal bool IsMatch(PSPropertyExpression expression)
            string expressionString = expression.ToString();
            return _wildcardPatterns.Any(pattern => pattern.IsMatch(expressionString));
        private readonly WildcardPattern[] _wildcardPatterns;

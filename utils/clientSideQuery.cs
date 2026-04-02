    /// Client-side filtering for
    /// 1) filtering that cannot be translated into a server-side query (i.e. when CimQuery.WildcardToWqlLikeOperand reports that it cannot translate into WQL)
    /// 2) detecting if all expected results have been received and giving friendly user errors otherwise (i.e. could not find process with name='foo';  details in Windows 8 Bugs: #60926)
    internal sealed class ClientSideQuery : QueryBuilder
        internal sealed class NotFoundError
            public NotFoundError()
                this.ErrorMessageGenerator = GetErrorMessageForNotFound;
            public NotFoundError(string propertyName, object propertyValue, bool wildcardsEnabled)
                this.PropertyValue = propertyValue;
                if (wildcardsEnabled)
                    if ((propertyValue is string propertyValueAsString) && (WildcardPattern.ContainsWildcardCharacters(propertyValueAsString)))
                        this.ErrorMessageGenerator =
                            (queryDescription, className) => GetErrorMessageForNotFound_ForWildcard(this.PropertyName, this.PropertyValue, className);
                            (queryDescription, className) => GetErrorMessageForNotFound_ForEquality(this.PropertyName, this.PropertyValue, className);
            public string PropertyName { get; }
            public object PropertyValue { get; }
            public Func<string, string, string> ErrorMessageGenerator { get; }
            private static string GetErrorMessageForNotFound(string queryDescription, string className)
                string message = string.Format(
                    CultureInfo.InvariantCulture, // queryDescription should already be in the right format - can use invariant culture here
                    CmdletizationResources.CimJob_NotFound_ComplexCase,
                    queryDescription,
                    className);
            private static string GetErrorMessageForNotFound_ForEquality(string propertyName, object propertyValue, string className)
                    CmdletizationResources.CimJob_NotFound_SimpleGranularCase_Equality,
            private static string GetErrorMessageForNotFound_ForWildcard(string propertyName, object propertyValue, string className)
                    CmdletizationResources.CimJob_NotFound_SimpleGranularCase_Wildcard,
        private abstract class CimInstanceFilterBase
            protected abstract bool IsMatchCore(CimInstance cimInstance);
            protected BehaviorOnNoMatch BehaviorOnNoMatch { get; set; }
            private bool HadMatches { get; set; }
            public bool IsMatch(CimInstance cimInstance)
                bool isMatch = this.IsMatchCore(cimInstance);
                this.HadMatches = this.HadMatches || isMatch;
                return isMatch;
            public virtual bool ShouldReportErrorOnNoMatches_IfMultipleFilters()
                switch (this.BehaviorOnNoMatch)
                    case BehaviorOnNoMatch.ReportErrors:
                    case BehaviorOnNoMatch.SilentlyContinue:
                    case BehaviorOnNoMatch.Default:
                        Dbg.Assert(false, "BehaviorOnNoMatch.Default should be handled by derived classes");
            public virtual IEnumerable<NotFoundError> GetNotFoundErrors_IfThisIsTheOnlyFilter()
                        if (this.HadMatches)
                            return Enumerable.Empty<NotFoundError>();
                            return new[] { new NotFoundError() };
        private abstract class CimInstancePropertyBasedFilter : CimInstanceFilterBase
            private readonly List<PropertyValueFilter> _propertyValueFilters = new();
            protected IEnumerable<PropertyValueFilter> PropertyValueFilters { get { return _propertyValueFilters; } }
            protected void AddPropertyValueFilter(PropertyValueFilter propertyValueFilter)
                _propertyValueFilters.Add(propertyValueFilter);
            protected override bool IsMatchCore(CimInstance cimInstance)
                bool isMatch = false;
                foreach (PropertyValueFilter propertyValueFilter in this.PropertyValueFilters)
                    if (propertyValueFilter.IsMatch(cimInstance))
                        isMatch = true;
                        if (this.BehaviorOnNoMatch == BehaviorOnNoMatch.SilentlyContinue)
        private sealed class CimInstanceRegularFilter : CimInstancePropertyBasedFilter
            public CimInstanceRegularFilter(string propertyName, IEnumerable allowedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
                var valueBehaviors = new HashSet<BehaviorOnNoMatch>();
                foreach (object allowedPropertyValue in allowedPropertyValues)
                    PropertyValueFilter filter =
                        new PropertyValueRegularFilter(
                            allowedPropertyValue,
                            wildcardsEnabled,
                            behaviorOnNoMatch);
                    this.AddPropertyValueFilter(filter);
                    valueBehaviors.Add(filter.BehaviorOnNoMatch);
                if (valueBehaviors.Count == 1)
                    this.BehaviorOnNoMatch = valueBehaviors.First();
                    this.BehaviorOnNoMatch = behaviorOnNoMatch;
            public override bool ShouldReportErrorOnNoMatches_IfMultipleFilters()
                        return this.PropertyValueFilters
                                   .Any(static f => !f.HadMatch && f.BehaviorOnNoMatch == BehaviorOnNoMatch.ReportErrors);
            public override IEnumerable<NotFoundError> GetNotFoundErrors_IfThisIsTheOnlyFilter()
                    if (propertyValueFilter.BehaviorOnNoMatch != BehaviorOnNoMatch.ReportErrors)
                    if (propertyValueFilter.HadMatch)
                    var propertyValueRegularFilter = (PropertyValueRegularFilter)propertyValueFilter;
                    yield return propertyValueRegularFilter.GetGranularNotFoundError();
        private sealed class CimInstanceExcludeFilter : CimInstancePropertyBasedFilter
            public CimInstanceExcludeFilter(string propertyName, IEnumerable excludedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
                if (behaviorOnNoMatch == BehaviorOnNoMatch.Default)
                    this.BehaviorOnNoMatch = BehaviorOnNoMatch.SilentlyContinue;
                foreach (object excludedPropertyValue in excludedPropertyValues)
                    this.AddPropertyValueFilter(
                        new PropertyValueExcludeFilter(
                            excludedPropertyValue,
                            behaviorOnNoMatch));
        private sealed class CimInstanceMinFilter : CimInstancePropertyBasedFilter
            public CimInstanceMinFilter(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
                    new PropertyValueMinFilter(
                        minPropertyValue,
        private sealed class CimInstanceMaxFilter : CimInstancePropertyBasedFilter
            public CimInstanceMaxFilter(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
                    new PropertyValueMaxFilter(
        private sealed class CimInstanceAssociationFilter : CimInstanceFilterBase
            public CimInstanceAssociationFilter(BehaviorOnNoMatch behaviorOnNoMatch)
                    this.BehaviorOnNoMatch = BehaviorOnNoMatch.ReportErrors;
                return true; // the fact that this method is getting called means that CIM found associated instances (i.e. by definition the argument *is* matching)
        internal abstract class PropertyValueFilter
            protected PropertyValueFilter(string propertyName, object expectedPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
                PropertyName = propertyName;
                _behaviorOnNoMatch = behaviorOnNoMatch;
                OriginalExpectedPropertyValue = expectedPropertyValue;
                CimTypedExpectedPropertyValue = CimValueConverter.ConvertFromDotNetToCim(expectedPropertyValue);
            public BehaviorOnNoMatch BehaviorOnNoMatch
                    if (_behaviorOnNoMatch == BehaviorOnNoMatch.Default)
                        _behaviorOnNoMatch = this.GetDefaultBehaviorWhenNoMatchesFound(this.CimTypedExpectedPropertyValue);
                    return _behaviorOnNoMatch;
            protected abstract BehaviorOnNoMatch GetDefaultBehaviorWhenNoMatchesFound(object cimTypedExpectedPropertyValue);
            private BehaviorOnNoMatch _behaviorOnNoMatch;
            public object CimTypedExpectedPropertyValue { get; }
            public object OriginalExpectedPropertyValue { get; }
            public bool HadMatch { get; private set; }
            public bool IsMatch(CimInstance o)
                CimProperty propertyInfo = o.CimInstanceProperties[PropertyName];
                if (propertyInfo == null)
                object actualPropertyValue = propertyInfo.Value;
                if (CimTypedExpectedPropertyValue == null)
                    HadMatch = HadMatch || (actualPropertyValue == null);
                    return actualPropertyValue == null;
                CimValueConverter.AssertIntrinsicCimValue(actualPropertyValue);
                CimValueConverter.AssertIntrinsicCimValue(CimTypedExpectedPropertyValue);
                actualPropertyValue = ConvertActualValueToExpectedType(actualPropertyValue, CimTypedExpectedPropertyValue);
                Dbg.Assert(IsSameType(actualPropertyValue, CimTypedExpectedPropertyValue), "Types of actual vs expected property value should always match");
                bool isMatch = this.IsMatchingValue(actualPropertyValue);
                HadMatch = HadMatch || isMatch;
            protected abstract bool IsMatchingValue(object actualPropertyValue);
            private object ConvertActualValueToExpectedType(object actualPropertyValue, object expectedPropertyValue)
                if (actualPropertyValue is string && expectedPropertyValue is not string)
                    actualPropertyValue = LanguagePrimitives.ConvertTo(actualPropertyValue, expectedPropertyValue.GetType(), CultureInfo.InvariantCulture);
                if (!IsSameType(actualPropertyValue, expectedPropertyValue))
                    var errorMessage = string.Format(
                        CmdletizationResources.CimJob_MismatchedTypeOfPropertyReturnedByQuery,
                        PropertyName,
                        actualPropertyValue.GetType().FullName,
                        expectedPropertyValue.GetType().FullName);
                    throw CimJobException.CreateWithoutJobContext(
                        "CimJob_PropertyTypeUnexpectedByClientSideQuery",
                        ErrorCategory.InvalidType);
                return actualPropertyValue;
            private static bool IsSameType(object actualPropertyValue, object expectedPropertyValue)
                if (actualPropertyValue == null)
                if (expectedPropertyValue == null)
                if (actualPropertyValue is TimeSpan || actualPropertyValue is DateTime)
                    return expectedPropertyValue is TimeSpan || expectedPropertyValue is DateTime;
                return actualPropertyValue.GetType() == expectedPropertyValue.GetType();
        internal class PropertyValueRegularFilter : PropertyValueFilter
            private readonly bool _wildcardsEnabled;
            public PropertyValueRegularFilter(string propertyName, object expectedPropertyValue, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
                : base(propertyName, expectedPropertyValue, behaviorOnNoMatch)
                _wildcardsEnabled = wildcardsEnabled;
            protected override BehaviorOnNoMatch GetDefaultBehaviorWhenNoMatchesFound(object cimTypedExpectedPropertyValue)
                if (!_wildcardsEnabled)
                    return BehaviorOnNoMatch.ReportErrors;
                    if (cimTypedExpectedPropertyValue is string expectedPropertyValueAsString && WildcardPattern.ContainsWildcardCharacters(expectedPropertyValueAsString))
                        return BehaviorOnNoMatch.SilentlyContinue;
            internal NotFoundError GetGranularNotFoundError()
                return new NotFoundError(this.PropertyName, this.OriginalExpectedPropertyValue, _wildcardsEnabled);
            protected override bool IsMatchingValue(object actualPropertyValue)
                if (_wildcardsEnabled)
                    return WildcardEqual(this.PropertyName, actualPropertyValue, this.CimTypedExpectedPropertyValue);
                    return NonWildcardEqual(this.PropertyName, actualPropertyValue, this.CimTypedExpectedPropertyValue);
            private static bool NonWildcardEqual(string propertyName, object actualPropertyValue, object expectedPropertyValue)
                // perform .NET-based, case-insensitive equality test for 1) characters and 2) strings
                if (expectedPropertyValue is char)
                    expectedPropertyValue = expectedPropertyValue.ToString();
                    actualPropertyValue = actualPropertyValue.ToString();
                if (expectedPropertyValue is string expectedPropertyValueAsString)
                    var actualPropertyValueAsString = (string)actualPropertyValue;
                    return actualPropertyValueAsString.Equals(expectedPropertyValueAsString, StringComparison.OrdinalIgnoreCase);
                // perform .NET based equality for everything else
                return actualPropertyValue.Equals(expectedPropertyValue);
            private static bool WildcardEqual(string propertyName, object actualPropertyValue, object expectedPropertyValue)
                string actualPropertyValueAsString;
                string expectedPropertyValueAsString;
                if (!LanguagePrimitives.TryConvertTo(actualPropertyValue, out actualPropertyValueAsString))
                if (!LanguagePrimitives.TryConvertTo(expectedPropertyValue, out expectedPropertyValueAsString))
                return WildcardPattern.Get(expectedPropertyValueAsString, WildcardOptions.IgnoreCase).IsMatch(actualPropertyValueAsString);
        internal sealed class PropertyValueExcludeFilter : PropertyValueRegularFilter
            public PropertyValueExcludeFilter(string propertyName, object expectedPropertyValue, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
                : base(propertyName, expectedPropertyValue, wildcardsEnabled, behaviorOnNoMatch)
                return !base.IsMatchingValue(actualPropertyValue);
        internal sealed class PropertyValueMinFilter : PropertyValueFilter
            public PropertyValueMinFilter(string propertyName, object expectedPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
                return ActualValueGreaterThanOrEqualToExpectedValue(this.PropertyName, actualPropertyValue, this.CimTypedExpectedPropertyValue);
            private static bool ActualValueGreaterThanOrEqualToExpectedValue(string propertyName, object actualPropertyValue, object expectedPropertyValue)
                    if (expectedPropertyValue is not IComparable expectedComparable)
                    return expectedComparable.CompareTo(actualPropertyValue) <= 0;
        internal sealed class PropertyValueMaxFilter : PropertyValueFilter
            public PropertyValueMaxFilter(string propertyName, object expectedPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
                return ActualValueLessThanOrEqualToExpectedValue(this.PropertyName, actualPropertyValue, this.CimTypedExpectedPropertyValue);
            private static bool ActualValueLessThanOrEqualToExpectedValue(string propertyName, object actualPropertyValue, object expectedPropertyValue)
                    if (actualPropertyValue is not IComparable actualComparable)
                    return actualComparable.CompareTo(expectedPropertyValue) <= 0;
        private int _numberOfResultsFromMi;
        private int _numberOfMatchingResults;
        private readonly List<CimInstanceFilterBase> _filters = new();
        private readonly object _myLock = new();
        #region "Public" interface for client-side filtering
        internal bool IsResultMatchingClientSideQuery(CimInstance result)
            lock (_myLock)
                _numberOfResultsFromMi++;
                if (_filters.All(f => f.IsMatch(result)))
                    _numberOfMatchingResults++;
        internal IEnumerable<NotFoundError> GenerateNotFoundErrors()
            if (_filters.Count > 1)
                if (_numberOfMatchingResults > 0)
                if (_filters.All(static f => !f.ShouldReportErrorOnNoMatches_IfMultipleFilters()))
            CimInstanceFilterBase filter = _filters.SingleOrDefault();
            if (filter != null)
                return filter.GetNotFoundErrors_IfThisIsTheOnlyFilter();
        #region QueryBuilder interface
            _filters.Add(new CimInstanceRegularFilter(propertyName, allowedPropertyValues, wildcardsEnabled, behaviorOnNoMatch));
            _filters.Add(new CimInstanceExcludeFilter(propertyName, excludedPropertyValues, wildcardsEnabled, behaviorOnNoMatch));
            _filters.Add(new CimInstanceMinFilter(propertyName, minPropertyValue, behaviorOnNoMatch));
            _filters.Add(new CimInstanceMaxFilter(propertyName, maxPropertyValue, behaviorOnNoMatch));
            _filters.Add(new CimInstanceAssociationFilter(behaviorOnNoMatch));

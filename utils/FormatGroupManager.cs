    /// Internal class to manage the grouping algorithm for the
    /// format-xxx commands.
    internal sealed class GroupingInfoManager
        /// Initialize with the grouping property data.
        /// <param name="groupingExpression">Name of the grouping property.</param>
        /// <param name="displayLabel">Display name of the property.</param>
        internal void Initialize(PSPropertyExpression groupingExpression, string displayLabel)
            _groupingKeyExpression = groupingExpression;
            _label = displayLabel;
        internal object CurrentGroupingKeyPropertyValue
            get { return _currentGroupingKeyPropertyValue; }
        internal string GroupingKeyDisplayName
                if (_label != null)
                    return _label;
                return _groupingKeyDisplayName;
        /// Compute the string value of the grouping property.
        /// <param name="so">Object to use to compute the property value.</param>
        /// <returns>True if there was an update.</returns>
        internal bool UpdateGroupingKeyValue(PSObject so)
            if (_groupingKeyExpression == null)
            List<PSPropertyExpressionResult> results = _groupingKeyExpression.GetValues(so);
            // if we have more that one match, we have to select the first one
            if (results.Count > 0 && results[0].Exception == null)
                // no exception got thrown, so we can update
                object newValue = results[0].Result;
                object oldValue = _currentGroupingKeyPropertyValue;
                _currentGroupingKeyPropertyValue = newValue;
                // now do the comparison
                bool update = !(IsEqual(_currentGroupingKeyPropertyValue, oldValue) ||
                                IsEqual(oldValue, _currentGroupingKeyPropertyValue));
                if (update && _label == null)
                    _groupingKeyDisplayName = results[0].ResolvedExpression.ToString();
                return update;
            // we had no matches or we could not get the value:
            // NOTICE: we need to do this to avoid starting a new group every time
            // there is a failure to read the grouping property.
            // For example, for AD, there are objects that throw when trying
            // to read the "distinguishedName" property (used by the brokered property "ParentPath)
        private static bool IsEqual(object first, object second)
            if (LanguagePrimitives.TryCompare(first, second, true, CultureInfo.CurrentCulture, out int result))
            // or an Exception was raised win Compare
            return string.Equals(firstString, secondString, StringComparison.CurrentCultureIgnoreCase);
        /// Value of the display label passed in.
        private string _label = null;
        /// Value of the current active grouping key.
        private string _groupingKeyDisplayName = null;
        /// Name of the current grouping key.
        private PSPropertyExpression _groupingKeyExpression = null;
        /// The current value of the grouping key.
        private object _currentGroupingKeyPropertyValue = AutomationNull.Value;

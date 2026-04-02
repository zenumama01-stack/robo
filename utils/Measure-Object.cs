    /// Class output by Measure-Object.
    public abstract class MeasureInfo
        /// Property name.
        public string Property { get; set; }
    public sealed class GenericMeasureInfo : MeasureInfo
        /// Initializes a new instance of the <see cref="GenericMeasureInfo"/> class.
        public GenericMeasureInfo()
            Average = Sum = Maximum = Minimum = StandardDeviation = null;
        /// Keeping track of number of objects with a certain property.
        public int Count { get; set; }
        /// The average of property values.
        public double? Average { get; set; }
        /// The sum of property values.
        public double? Sum { get; set; }
        /// The max of property values.
        public double? Maximum { get; set; }
        /// The min of property values.
        public double? Minimum { get; set; }
        /// The Standard Deviation of property values.
        public double? StandardDeviation { get; set; }
    /// This class is created to make 'Measure-Object -MAX -MIN' work with ANYTHING that supports 'CompareTo'.
    /// GenericMeasureInfo class is shipped with PowerShell V2. Fixing this bug requires, changing the type of
    /// Maximum and Minimum properties which would be a breaking change. Hence created a new class to not
    /// have an appcompat issues with PS V2.
    public sealed class GenericObjectMeasureInfo : MeasureInfo
        /// Initializes a new instance of the <see cref="GenericObjectMeasureInfo"/> class.
        /// Default ctor.
        public GenericObjectMeasureInfo()
            Average = Sum = StandardDeviation = null;
            Maximum = Minimum = null;
    public sealed class TextMeasureInfo : MeasureInfo
        /// Initializes a new instance of the <see cref="TextMeasureInfo"/> class.
        public TextMeasureInfo()
            Lines = Words = Characters = null;
        public int? Lines { get; set; }
        public int? Words { get; set; }
        public int? Characters { get; set; }
    /// Measure object cmdlet.
    [Cmdlet(VerbsDiagnostic.Measure, "Object", DefaultParameterSetName = GenericParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096617", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(GenericMeasureInfo), typeof(TextMeasureInfo), typeof(GenericObjectMeasureInfo))]
    public sealed class MeasureObjectCommand : PSCmdlet
        /// Dictionary to be used by Measure-Object implementation.
        /// Keys are strings. Keys are compared with OrdinalIgnoreCase.
        /// <typeparam name="TValue">Value type.</typeparam>
        private sealed class MeasureObjectDictionary<TValue> : Dictionary<string, TValue>
            where TValue : new()
            /// Initializes a new instance of the <see cref="MeasureObjectDictionary{TValue}"/> class.
            internal MeasureObjectDictionary() : base(StringComparer.OrdinalIgnoreCase)
            /// Attempt to look up the value associated with the
            /// the specified key. If a value is not found, associate
            /// the key with a new value created via the value type's
            /// default constructor.
            /// <param name="key">The key to look up.</param>
            /// The existing value, or a newly-created value.
            public TValue EnsureEntry(string key)
                TValue val;
                if (!TryGetValue(key, out val))
                    val = new TValue();
                    this[key] = val;
        /// Convenience class to track statistics without having
        /// to maintain two sets of MeasureInfo and constantly checking
        /// what mode we're in.
        private sealed class Statistics
            // Common properties
            internal int count = 0;
            // Generic/Numeric statistics
            internal double sum = 0.0;
            internal double sumPrevious = 0.0;
            internal double variance = 0.0;
            internal object max = null;
            internal object min = null;
            // Text statistics
            internal int characters = 0;
            internal int words = 0;
            internal int lines = 0;
        /// Initializes a new instance of the <see cref="MeasureObjectCommand"/> class.
        /// Default constructor.
        public MeasureObjectCommand()
        #region Common parameters in both sets
        /// Incoming object.
        /// Properties to be examined.
        public PSPropertyExpression[] Property { get; set; }
        #endregion Common parameters in both sets
        /// Set to true if Standard Deviation is to be returned.
        [Parameter(ParameterSetName = GenericParameterSet)]
        public SwitchParameter StandardDeviation
                return _measureStandardDeviation;
                _measureStandardDeviation = value;
        private bool _measureStandardDeviation;
        /// Set to true is Sum is to be returned.
        public SwitchParameter Sum
                return _measureSum;
                _measureSum = value;
        private bool _measureSum;
        /// Gets or sets the value indicating if all statistics should be returned.
        public SwitchParameter AllStats
                return _allStats;
                _allStats = value;
        private bool _allStats;
        /// Set to true is Average is to be returned.
        public SwitchParameter Average
                return _measureAverage;
                _measureAverage = value;
        private bool _measureAverage;
        /// Set to true is Max is to be returned.
        public SwitchParameter Maximum
                return _measureMax;
                _measureMax = value;
        private bool _measureMax;
        /// Set to true is Min is to be returned.
        public SwitchParameter Minimum
                return _measureMin;
                _measureMin = value;
        private bool _measureMin;
        #region TextMeasure ParameterSet
        [Parameter(ParameterSetName = TextParameterSet)]
        public SwitchParameter Line
                return _measureLines;
                _measureLines = value;
        private bool _measureLines = false;
        public SwitchParameter Word
                return _measureWords;
                _measureWords = value;
        private bool _measureWords = false;
        public SwitchParameter Character
                return _measureCharacters;
                _measureCharacters = value;
        private bool _measureCharacters = false;
                return _ignoreWhiteSpace;
                _ignoreWhiteSpace = value;
        private bool _ignoreWhiteSpace;
        #endregion TextMeasure ParameterSet
        #endregion Command Line Switches
        /// Which parameter set the Cmdlet is in.
        private bool IsMeasuringGeneric
                return string.Equals(ParameterSetName, GenericParameterSet, StringComparison.Ordinal);
        /// Does the begin part of the cmdlet.
            // Sets all other generic parameters to true to get all statistics.
            if (_allStats)
                _measureSum = _measureStandardDeviation = _measureAverage = _measureMax = _measureMin = true;
            // finally call the base class.
        /// Collect data about each record that comes in.
        /// Side effects: Updates totalRecordCount.
            if (InputObject == null || InputObject == AutomationNull.Value)
            _totalRecordCount++;
            if (Property == null)
                AnalyzeValue(null, InputObject.BaseObject);
                AnalyzeObjectProperties(InputObject);
        /// Analyze an object on a property-by-property basis instead
        /// of as a simple value.
        /// Side effects: Updates statistics.
        /// <param name="inObj">The object to analyze.</param>
        private void AnalyzeObjectProperties(PSObject inObj)
            // Keep track of which properties are counted for an
            // input object so that repeated properties won't be
            // counted twice.
            MeasureObjectDictionary<object> countedProperties = new();
            // First iterate over the user-specified list of
            // properties...
            foreach (var expression in Property)
                List<PSPropertyExpression> resolvedNames = expression.ResolveNames(inObj);
                if (resolvedNames == null || resolvedNames.Count == 0)
                    // Insert a blank entry so we can track
                    // property misses in EndProcessing.
                    if (!expression.HasWildCardCharacters)
                        string propertyName = expression.ToString();
                        _statistics.EnsureEntry(propertyName);
                // Each property value can potentially refer
                // to multiple properties via globbing. Iterate over
                // the actual property names.
                    string propertyName = resolvedName.ToString();
                    // skip duplicated properties
                    if (countedProperties.ContainsKey(propertyName))
                    List<PSPropertyExpressionResult> tempExprRes = resolvedName.GetValues(inObj);
                    if (tempExprRes == null || tempExprRes.Count == 0)
                        // Shouldn't happen - would somehow mean
                        // that the property went away between when
                        // we resolved it and when we tried to get its
                        // value.
                    AnalyzeValue(propertyName, tempExprRes[0].Result);
                    // Remember resolved propertyNames that have been counted
                    countedProperties[propertyName] = null;
        /// Analyze a value for generic/text statistics.
        /// Side effects: Updates statistics. May set nonNumericError.
        /// <param name="propertyName">The property this value corresponds to.</param>
        /// <param name="objValue">The value to analyze.</param>
        private void AnalyzeValue(string propertyName, object objValue)
            propertyName ??= thisObject;
            Statistics stat = _statistics.EnsureEntry(propertyName);
            // Update common properties.
            stat.count++;
            if (_measureCharacters || _measureWords || _measureLines)
                string strValue = (objValue == null) ? string.Empty : objValue.ToString();
                AnalyzeString(strValue, stat);
            if (_measureAverage || _measureSum || _measureStandardDeviation)
                double numValue = 0.0;
                if (!LanguagePrimitives.TryConvertTo(objValue, out numValue))
                    _nonNumericError = true;
                        PSTraceSource.NewInvalidOperationException(MeasureObjectStrings.NonNumericInputObject, objValue),
                        "NonNumericInputObject",
                        ErrorCategory.InvalidType,
                        objValue);
                AnalyzeNumber(numValue, stat);
            // Measure-Object -MAX -MIN should work with ANYTHING that supports CompareTo
            if (_measureMin)
                stat.min = Compare(objValue, stat.min, true);
            if (_measureMax)
                stat.max = Compare(objValue, stat.max, false);
        /// Compare is a helper function used to find the min/max between the supplied input values.
        /// <param name="objValue">
        /// Current input value.
        /// <param name="statMinOrMaxValue">
        /// Current minimum or maximum value in the statistics.
        /// <param name="isMin">
        /// Indicates if minimum or maximum value has to be found.
        /// If true is passed in then the minimum of the two values would be returned.
        /// If false is passed in then maximum of the two values will be returned.</param>
        private static object Compare(object objValue, object statMinOrMaxValue, bool isMin)
            object currentValue = objValue;
            object statValue = statMinOrMaxValue;
            double temp;
            currentValue = ((objValue != null) && LanguagePrimitives.TryConvertTo<double>(objValue, out temp)) ? temp : currentValue;
            statValue = ((statValue != null) && LanguagePrimitives.TryConvertTo<double>(statValue, out temp)) ? temp : statValue;
            if (currentValue != null && statValue != null && !currentValue.GetType().Equals(statValue.GetType()))
                currentValue = PSObject.AsPSObject(currentValue).ToString();
                statValue = PSObject.AsPSObject(statValue).ToString();
            if (statValue == null)
                return objValue;
            int comparisonResult = LanguagePrimitives.Compare(statValue, currentValue, ignoreCase: false, CultureInfo.CurrentCulture);
            return (isMin ? comparisonResult : -comparisonResult) > 0
                ? objValue
                : statMinOrMaxValue;
        /// Class contains util static functions.
        private static class TextCountUtilities
            /// Count chars in inStr.
            /// <param name="inStr">String whose chars are counted.</param>
            /// <param name="ignoreWhiteSpace">True to discount white space.</param>
            /// <returns>Number of chars in inStr.</returns>
            internal static int CountChar(string inStr, bool ignoreWhiteSpace)
                if (string.IsNullOrEmpty(inStr))
                if (!ignoreWhiteSpace)
                    return inStr.Length;
                int len = 0;
                foreach (char c in inStr)
                    if (!char.IsWhiteSpace(c))
            /// Count words in inStr.
            /// <param name="inStr">String whose words are counted.</param>
            /// <returns>Number of words in inStr.</returns>
            internal static int CountWord(string inStr)
                int wordCount = 0;
                bool wasAWhiteSpace = true;
                    if (char.IsWhiteSpace(c))
                        wasAWhiteSpace = true;
                        if (wasAWhiteSpace)
                            wordCount++;
                        wasAWhiteSpace = false;
                return wordCount;
            /// Count lines in inStr.
            /// <param name="inStr">String whose lines are counted.</param>
            /// <returns>Number of lines in inStr.</returns>
            internal static int CountLine(string inStr)
                int numberOfLines = 0;
                    if (c == '\n')
                        numberOfLines++;
                // 'abc\nd' has two lines
                // but 'abc\n' has one line
                if (inStr[inStr.Length - 1] != '\n')
                return numberOfLines;
        /// Update text statistics.
        /// <param name="strValue">The text to analyze.</param>
        /// <param name="stat">The Statistics object to update.</param>
        private void AnalyzeString(string strValue, Statistics stat)
            if (_measureCharacters)
                stat.characters += TextCountUtilities.CountChar(strValue, _ignoreWhiteSpace);
            if (_measureWords)
                stat.words += TextCountUtilities.CountWord(strValue);
            if (_measureLines)
                stat.lines += TextCountUtilities.CountLine(strValue);
        /// Update number statistics.
        /// <param name="numValue">The number to analyze.</param>
        private void AnalyzeNumber(double numValue, Statistics stat)
            if (_measureSum || _measureAverage || _measureStandardDeviation)
                stat.sumPrevious = stat.sum;
                stat.sum += numValue;
            if (_measureStandardDeviation && stat.count > 1)
                // Based off of iterative method of calculating variance on
                // https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm
                double avgPrevious = stat.sumPrevious / (stat.count - 1);
                stat.variance *= (stat.count - 2.0) / (stat.count - 1);
                stat.variance += (numValue - avgPrevious) * (numValue - avgPrevious) / stat.count;
        /// WriteError when a property is not found.
        /// <param name="propertyName">The missing property.</param>
        /// <param name="errorId">The error ID to write.</param>
        private void WritePropertyNotFoundError(string propertyName, string errorId)
            Diagnostics.Assert(Property != null, "no property and no InputObject should have been addressed");
                    PSTraceSource.NewArgumentException(propertyName),
            errorRecord.ErrorDetails = new ErrorDetails(
                this, "MeasureObjectStrings", "PropertyNotFound", propertyName);
        /// Output collected statistics.
        /// Side effects: Updates statistics. Writes objects to stream.
            // Fix for 917114: If Property is not set,
            // and we aren't passed any records at all,
            // output 0s to emulate wc behavior.
            if (_totalRecordCount == 0 && Property == null)
                _statistics.EnsureEntry(thisObject);
            foreach (string propertyName in _statistics.Keys)
                Statistics stat = _statistics[propertyName];
                if (stat.count == 0 && Property != null)
                    if (Context.IsStrictVersion(2))
                        string errorId = (IsMeasuringGeneric) ? "GenericMeasurePropertyNotFound" : "TextMeasurePropertyNotFound";
                        WritePropertyNotFoundError(propertyName, errorId);
                MeasureInfo mi = null;
                if (IsMeasuringGeneric)
                    if ((stat.min == null || LanguagePrimitives.TryConvertTo<double>(stat.min, out temp)) &&
                        (stat.max == null || LanguagePrimitives.TryConvertTo<double>(stat.max, out temp)))
                        mi = CreateGenericMeasureInfo(stat, true);
                        mi = CreateGenericMeasureInfo(stat, false);
                    mi = CreateTextMeasureInfo(stat);
                // Set common properties.
                if (Property != null)
                    mi.Property = propertyName;
                WriteObject(mi);
        /// Create a MeasureInfo object for generic stats.
        /// <param name="stat">The statistics to use.</param>
        /// <param name="shouldUseGenericMeasureInfo"></param>
        /// <returns>A new GenericMeasureInfo object.</returns>
        private MeasureInfo CreateGenericMeasureInfo(Statistics stat, bool shouldUseGenericMeasureInfo)
            double? sum = null;
            double? average = null;
            double? StandardDeviation = null;
            object max = null;
            object min = null;
            if (!_nonNumericError)
                if (_measureSum)
                    sum = stat.sum;
                if (_measureAverage && stat.count > 0)
                    average = stat.sum / stat.count;
                if (_measureStandardDeviation)
                    StandardDeviation = Math.Sqrt(stat.variance);
                if (shouldUseGenericMeasureInfo && (stat.max != null))
                    LanguagePrimitives.TryConvertTo<double>(stat.max, out temp);
                    max = temp;
                    max = stat.max;
                if (shouldUseGenericMeasureInfo && (stat.min != null))
                    LanguagePrimitives.TryConvertTo<double>(stat.min, out temp);
                    min = temp;
                    min = stat.min;
            if (shouldUseGenericMeasureInfo)
                GenericMeasureInfo gmi = new();
                gmi.Count = stat.count;
                gmi.Sum = sum;
                gmi.Average = average;
                gmi.StandardDeviation = StandardDeviation;
                if (max != null)
                    gmi.Maximum = (double)max;
                if (min != null)
                    gmi.Minimum = (double)min;
                return gmi;
                GenericObjectMeasureInfo gomi = new();
                gomi.Count = stat.count;
                gomi.Sum = sum;
                gomi.Average = average;
                gomi.Maximum = max;
                gomi.Minimum = min;
                return gomi;
        /// Create a MeasureInfo object for text stats.
        /// <returns>A new TextMeasureInfo object.</returns>
        private TextMeasureInfo CreateTextMeasureInfo(Statistics stat)
            TextMeasureInfo tmi = new();
                tmi.Characters = stat.characters;
                tmi.Words = stat.words;
                tmi.Lines = stat.lines;
            return tmi;
        /// The observed statistics keyed by property name.
        /// If Property is not set, then the key used will be the value of thisObject.
        private readonly MeasureObjectDictionary<Statistics> _statistics = new();
        /// Whether or not a numeric conversion error occurred.
        /// If true, then average/sum/standard deviation will not be output.
        private bool _nonNumericError = false;
        /// The total number of records encountered.
        private int _totalRecordCount = 0;
        /// Parameter set name for measuring objects.
        private const string GenericParameterSet = "GenericMeasure";
        /// Parameter set name for measuring text.
        private const string TextParameterSet = "TextMeasure";
        /// Key that statistics are stored under when Property is not set.
        private const string thisObject = "$_";

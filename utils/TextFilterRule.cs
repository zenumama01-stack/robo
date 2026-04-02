    /// The TextFilterRule class supports derived rules by offering services for
    /// evaluating string operations.
    public abstract class TextFilterRule : SingleValueComparableValueFilterRule<string>
        /// Gets a regex pattern that describes a word boundary that can include symbols.
        protected static readonly string WordBoundaryRegexPattern = @"(^|$|\W|\b)";
        private bool ignoreCase;
        private bool cultureInvariant;
        /// Gets or sets whether to ignore case when evaluating.
        public bool IgnoreCase
                return this.ignoreCase;
                this.ignoreCase = value;
        /// Gets or sets whether culture differences in language are ignored when evaluating.
        public bool CultureInvariant
                return this.cultureInvariant;
                this.cultureInvariant = value;
        /// Initializes a new instance of the <see cref="TextFilterRule"/> class.
        protected TextFilterRule()
            this.IgnoreCase = true;
            this.CultureInvariant = false;
        /// Initializes a new instance of the  <see cref="TextFilterRule"/> class.
        protected TextFilterRule(TextFilterRule source)
            this.IgnoreCase = source.IgnoreCase;
            this.CultureInvariant = source.CultureInvariant;
        /// Gets the current value and determines whether it should be evaluated as an exact match.
        /// <param name="evaluateAsExactMatch">Whether the current value should be evaluated as an exact match.</param>
        /// <returns>The current value.</returns>
        protected internal string GetParsedValue(out bool evaluateAsExactMatch)
            var parsedValue = this.Value.GetCastValue();
            // Consider it an exact-match value if it starts with a quote; trailing quotes and other requirements can be added later if need be \\
            evaluateAsExactMatch = parsedValue.StartsWith("\"", StringComparison.Ordinal);
            // If it's an exact-match value, remove quotes and use the exact-match pattern \\
            if (evaluateAsExactMatch)
                parsedValue = parsedValue.Replace("\"", string.Empty);
            return parsedValue;
        /// Gets a regular expression pattern based on the current value and the specified patterns.
        /// If the current value is an exact-match string, <paramref name="exactMatchPattern"/> will be used; otherwise, <paramref name="pattern"/> will be used.
        /// <param name="pattern">The pattern to use if the current value is not an exact-match string. The pattern must contain a <c>{0}</c> token.</param>
        /// <param name="exactMatchPattern">The pattern to use if the current value is an exact-match string. The pattern must contain a <c>{0}</c> token.</param>
        /// <returns>A regular expression pattern based on the current value and the specified patterns.</returns>
        protected internal string GetRegexPattern(string pattern, string exactMatchPattern)
            ArgumentNullException.ThrowIfNull(pattern);
            ArgumentNullException.ThrowIfNull(exactMatchPattern);
            bool evaluateAsExactMatch;
            string value = this.GetParsedValue(out evaluateAsExactMatch);
                pattern = exactMatchPattern;
            value = Regex.Escape(value);
            // Format the pattern using the specified data \\
            return string.Format(CultureInfo.InvariantCulture, pattern, value);
        /// Gets a <see cref="RegexOptions"/> object that matches the values of <see cref="IgnoreCase"/> and <see cref="CultureInvariant"/>.
        /// <returns>A <see cref="RegexOptions"/> object that matches the values of <see cref="IgnoreCase"/> and <see cref="CultureInvariant"/>.</returns>
        protected internal RegexOptions GetRegexOptions()
            RegexOptions options = RegexOptions.None;
            if (this.IgnoreCase)
                options |= RegexOptions.IgnoreCase;
            if (this.CultureInvariant)
                options |= RegexOptions.CultureInvariant;
            return options;
        /// Gets a value indicating whether the specified data matches one of the specified patterns.
        /// <returns><c>true</c> if the specified data matches one of the specified patterns; otherwise, <c>false</c>.</returns>
        protected internal bool ExactMatchEvaluate(string data, string pattern, string exactMatchPattern)
            var parsedPattern = this.GetRegexPattern(pattern, exactMatchPattern);
            var options = this.GetRegexOptions();
            return Regex.IsMatch(data, parsedPattern, options);
